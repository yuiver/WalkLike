using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class ResourceManager : GSingleton<ResourceManager>
{

    // 실제 로드한 리소스.
    Dictionary<string, Object> _resources = new Dictionary<string, Object>();
    // 비동기 리소스 진행 상황.
    Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();
    // 한번에 인스턴스 하기 위한 리스트
    List<Object> _loadedResources = new List<Object>();

    private int _maxConcurrentLoads = 10;   // 동시 로드 가능 최대개수
    private int _currentConcurrentLoads = 0;    // 현재 실행중인 로드 개수
    private Queue<Action> _loadQueue = new Queue<Action>(); // 대기중인 로드 작업 대기열

    public int HandlesCount = 0;

    #region 리소스
    public void LoadAsync<T>(string key, Action<T> callback = null) where T : Object
    {
        // 캐시 확인.
        if (_resources.TryGetValue(key, out Object resource))
        {
            callback?.Invoke(resource as T);
            return;
        }

        // 로딩은 시작했지만 완료되지 않았다면, 콜백만 추가.
        if (_handles.ContainsKey(key))
        {
            _handles[key].Completed += (op) => { callback?.Invoke(op.Result as T); };
            return;
        }

        // 리소스 비동기 로딩 시작.
        if (_currentConcurrentLoads < _maxConcurrentLoads)
        {
            StartLoading<T>(key, callback);
        }
        else
        { 
            _loadQueue.Enqueue(() => StartLoading<T>(key, callback));
        }
    }
    //! LoadAsync의 한번에 로드하는 버전
    public void LoadAsync<T>(List<string> keys, Action<List<T>> callback = null) where T : Object
    {
        List<T> loadedObjects = new List<T>(); // 로드된 오브젝트를 담을 리스트

        // 모든 키를 로드하는 코루틴을 실행
        StartCoroutine(LoadKeysCoroutine());

        IEnumerator LoadKeysCoroutine()
        {
            // 키 리스트를 순회하며 로드
            foreach (string key in keys)
            {
                // 키에 해당하는 오브젝트를 로드
                yield return LoadKeyCoroutine(key);
            }

            // 모든 오브젝트 로드 완료 후 콜백 호출
            callback?.Invoke(loadedObjects);
        }

        IEnumerator LoadKeyCoroutine(string key)
        {
            // 캐시 확인
            if (_resources.TryGetValue(key, out Object resource))
            {
                loadedObjects.Add(resource as T); // 이미 캐싱되어 있으면 리스트에 추가
            }
            else if (_handles.ContainsKey(key))
            {
                // 이미 로드가 시작되어 있으면 완료 대기 후 리스트에 추가
                yield return new WaitUntil(() => _handles[key].IsDone);
                loadedObjects.Add(_handles[key].Result as T);
            }
            else
            {
                // 리소스 비동기 로딩 시작.
                if (_currentConcurrentLoads < _maxConcurrentLoads)
                {
                    // 새 핸들 생성하여 로드 시작
                    AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
                    _handles.Add(key, handle);
                    _currentConcurrentLoads++;

                    yield return handle;

                    // 로드 완료 후 리스트에 추가
                    loadedObjects.Add(handle.Result);
                }
                else
                {
                    // 큐에 로드 요청 추가
                    _loadQueue.Enqueue(() => StartLoading(key));
                    yield return new WaitUntil(() => _handles.ContainsKey(key) && _handles[key].IsDone);

                    // 큐에서 로드 완료 후 리스트에 추가
                    loadedObjects.Add(_handles[key].Result as T);
                }
            }
        }

        void StartLoading(string key)
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            _handles.Add(key, handle);

            handle.Completed += (op) => {
                _currentConcurrentLoads--;
                _handles.Remove(key);
            };
        }
    }

    // 비동기 로드의 최대 개수를 제한하기 위해 추가
    private void StartLoading<T>(string key, Action<T> callback) where T : Object
    {
        _handles.Add(key, Addressables.LoadAssetAsync<T>(key));
        _currentConcurrentLoads++;
        HandlesCount++;

        _handles[key].Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _resources.Add(key, op.Result as Object);
                callback?.Invoke(op.Result as T);
            }
            else
            {
                GFunc.Log($"Failed to load resource with key : {key}");
            }
            // 작업 완료시 현재 로드 개수를 감소시키고 대기열에서 다음 작업을 시작
            _currentConcurrentLoads--;
            HandlesCount--;

            if (_loadQueue.Count > 0)
            { 
                Action nextLoad = _loadQueue.Dequeue();
                nextLoad();
            }
        };
    }
    public float GetLoadProgress(string key)
    {
        if (_handles.TryGetValue(key, out AsyncOperationHandle handle))
        {
            return handle.PercentComplete;
        }
        return 0.0f;
    }

    public float GetTotalLoadProgress()
    {
        if (_handles.Count == 0)
        {
            return 0.0f;
        }

        float totalProgress = 0.0f;

        foreach (var handle in _handles.Values)
        {
            totalProgress += handle.PercentComplete;
        }

        return totalProgress / _handles.Count;
    }



    public void Release(string key)
    {
        if (_resources.TryGetValue(key, out Object resource) == false)
            return;

        _resources.Remove(key);

        if (_handles.TryGetValue(key, out AsyncOperationHandle handle))
            Addressables.Release(handle);

        _handles.Remove(key);
    }

    public void Clear()
    {
        _resources.Clear();

        foreach (var handle in _handles.Values)
            Addressables.Release(handle);

        _handles.Clear();
    }
    #endregion

    #region 프리팹
    public void Instantiate(string key, Transform parent = null, Action<GameObject> callback = null)
    {
        LoadAsync<GameObject>(key, (prefab) =>
        {
            GameObject go = GameObject.Instantiate(prefab, parent);
            go.name = prefab.name;
            go.transform.localPosition = prefab.transform.position;
            callback?.Invoke(go);
        });
        //Addressables.InstantiateAsync(key, parent).Completed += (go) => 
        //{ 
        //	callback?.Invoke(go.Result); 
        //};
    }
    public GameObject Instantiate(GameObject go, Transform parent = null)
    {
        GameObject prefab = GameObject.Instantiate(go, parent);
        prefab.name = go.name;
        prefab.transform.localPosition = go.transform.position;

        return prefab;
    }
    public void Destroy(GameObject go, float seconds = 0.0f)
    {
        Object.Destroy(go, seconds);

        //if (seconds == 0.0f)
        //{
        //    Addressables.ReleaseInstance(go);
        //}
        //else
        //{
        //    StartCoroutine(CoDestroyAfter(go, seconds));
        //}
    }

    IEnumerator CoDestroyAfter(GameObject go, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Addressables.ReleaseInstance(go);
    }
    #endregion
}
