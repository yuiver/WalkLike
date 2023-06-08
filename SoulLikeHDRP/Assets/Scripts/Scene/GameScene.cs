using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    public Transform mapRoot = default;
    public Transform spawnPoint = default;
    private GameObject _mainCamera;
    public GameObject MainCamera
    {
        get { return _mainCamera; }
        set { _mainCamera = value; }
    }

    private GameObject _freeLook;
    public GameObject FreeLook
    {
        get { return _freeLook; }
        set { _freeLook = value; }
    }

    private GameObject _player;
    public GameObject Player
    {
        get { return _player; }
        set { _player = value; }
    }
    protected override bool Init()
    {
        if (base.Init() == false)
            return false;

        SceneType = SceneType.GameScene;
        ResourceManager.Instance.Instantiate("x1y2", mapRoot);

        ResourceManager.Instance.LoadAsync<GameObject>(new List<string> { "MainCamera", "FreeLook", "Player" }, (loadedPrefabs) =>
        {
            // 모든 프리팹이 로드된 후에 실행되는 콜백
            _mainCamera = ResourceManager.Instance.Instantiate(loadedPrefabs[0], spawnPoint);
            _freeLook = ResourceManager.Instance.Instantiate(loadedPrefabs[1], spawnPoint);
            _player = ResourceManager.Instance.Instantiate(loadedPrefabs[2], spawnPoint);
        });

        StartCoroutine(CacheResources());
        StartCoroutine(CoWaitLoad());

        return true;
    }
    IEnumerator CoWaitLoad()
    {
        while (GameManager.Instance.IsLoaded == false)
            yield return null;
    }
    private IEnumerator CacheResources()
    {
        // 로드한 리소스 리스트가 비어있는 경우에만 캐싱을 시도합니다.
        while (_mainCamera == null || _freeLook == null || _player == null)
        {
            yield return null;
        }

        // GameManager에 컴포넌트를 할당합니다.
        GameManager.Instance.freeLookController = _mainCamera.GetComponent<FreeLookController>();
        GameManager.Instance.playerController = _player.GetComponent<PlayerController>();
        GameManager.Instance.freeLookCamera = _freeLook.GetComponent<CinemachineFreeLook>();
        GameManager.Instance.playerController.mainCamera = _mainCamera.GetComponent<Camera>();
        GameManager.Instance.freeLookController.playerViewPoint = GameManager.Instance.playerController.transform.GetChild(0);
        Debug.Log($"Resource : {_mainCamera},{_freeLook},{_player}");
    }
}
