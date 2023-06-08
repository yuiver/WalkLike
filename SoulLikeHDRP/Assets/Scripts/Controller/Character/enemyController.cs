using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class enemyController : MonoBehaviour
{
    public GameObject findobjs = default;
    public GameObject targetObj = default;
    public Terrain terrain = default;
    
    [ShowInInspector]
    private HashSet<Vector3> obstacles = new HashSet<Vector3>();

    public GameObject pivot; 

    public float speed = 1f;
    public float rotationSpeed = 1f;
    public List<GameObject> obstacleObjects = new List<GameObject>();

    [ShowInInspector]
    private Queue<Vector3> pathPoints;
    private bool isTargetMoving = false;
    private bool isSearching = false;

    // 길을 찾아서 이동하는 코루틴을 원하는때 멈추기 위해 따로 캐싱해서 사용한다.
    Coroutine PathFollow = default;
    // Start is called before the first frame update
    private void OnCollisionEnter(Collision collision)
    {
        Terrain tempTerrain = collision.gameObject.GetComponent<Terrain>();
        // 이런식으로 return해버린다면 
        if (tempTerrain == null) {return;}
        if (tempTerrain.gameObject.CompareTag("Ground") == false) {return;}

        terrain = tempTerrain;
    }
    private void Awake()
    {
        // 각 장애물에 대해 AddObstacle 메서드를 호출합니다.
        foreach (GameObject obstacle in obstacleObjects)
        {
            AStarManager.Instance.AddObstacle(obstacle, 2.0f);
        }
    }

    void Start()
    {
        findobjs = this.gameObject;
        pivot = this.gameObject;
    }
    void FixedUpdate()
    {
        //업데이트에서 null reference Exception을 방지하기 위해서 추가해둔 방어로직
        Debug.Log("시작하기도 전에 터짐");
        if (GameManager.Instance.playerController == null || terrain == null) { return; }
        if (targetObj == null)
        {
            targetObj = GameManager.Instance.playerController.gameObject;
            Debug.Log("타겟 설정");
        }
        float tempDistance = Vector3.Distance(targetObj.transform.position, findobjs.transform.position);
        Debug.Log("타겟과 거리 30 이상이면 여기서 종료함");
        if (tempDistance > 30) { return; }
        Debug.Log("타겟과 거리가 3 이하라면 여기서 종료함");
        if (tempDistance < 3) { return; }

        Debug.Log("그냥 찾겠음 시발");
        //RePathFind();
        DoPathFind();

    }

    void DoPathFind()
    {
        pathPoints = new Queue<Vector3>();

        float findobjsTerrainHeight = terrain.SampleHeight(findobjs.transform.position);
        findobjs.transform.position = new Vector3(findobjs.transform.position.x, findobjsTerrainHeight, findobjs.transform.position.z);

        float targetObjTerrainHeight = terrain.SampleHeight(targetObj.transform.position);
        targetObj.transform.position = new Vector3(targetObj.transform.position.x, targetObjTerrainHeight, targetObj.transform.position.z);

        // 장애물 게임 오브젝트를 태그를 사용하여 찾습니다.
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        if (obstacles.Length > 0)
        {
           foreach (GameObject obstacle in obstacles)
            {
                // 장애물 주변 2 유닛 범위를 추가합니다.
                AStarManager.Instance.AddObstacle(obstacle, 2.0f);
            }
        }
        else
        {
            Debug.LogWarning("Obstacle objects not found. Make sure the objects have the correct tag.");
        }

        findobjs = this.gameObject;
        AStarManager.Instance.StartPathFinding(findobjs, targetObj, terrain);
    }
    //! 대상의 위치가 바뀔 경우를 생각해서 기존의 길 찾기를 중지하고 다시 탐색을 시작하는 함수를 구현
    private void RePathFind()
    {
        // 탐색 대상의 위치를 업데이트하고 현재 위치부터 다시 탐색 시작
        float targetObjTerrainHeight = terrain.SampleHeight(targetObj.transform.position);
        targetObj.transform.position = new Vector3(targetObj.transform.position.x, targetObjTerrainHeight, targetObj.transform.position.z);
        // 기존 경로를 정지하고 코루틴을 종료
        if (PathFollow != null)
        { 
            StopCoroutine(PathFollow);
        }
        // 새 경로 찾기 시작
        DoPathFind();
    }

    public void SetPath(List<Vector3> path)
    {
        pathPoints.Clear();
        foreach (var point in path)
        {
            pathPoints.Enqueue(point);
        }
        //Debug.Log($"Path Points: {string.Join(", ", pathPoints)}");
        PathFollow = StartCoroutine(FollowPath());
    }

    private IEnumerator FollowPath()
    {
        while (pathPoints.Count > 0)
        {
            Vector3 target = pathPoints.Peek();
            target.y = GetTerrainHeight(target);
            float step = speed * Time.deltaTime;

            Vector3 nextPosition = Vector3.MoveTowards(pivot.transform.position, target, step);
            nextPosition.y = GetTerrainHeight(nextPosition);

            pivot.transform.position = nextPosition;
            transform.position = nextPosition;
            transform.LookAt(target);

            //Debug.Log($"Current Position: {transform.position}, Target Position: {target}, Distance: {Vector3.Distance(transform.position, target)}");

            if (Vector3.Distance(pivot.transform.position, target) < 0.1f)
            {
                pathPoints.Dequeue();
            }
            yield return null;
        }
    }
    private IEnumerator DelaySearch()
    {
        yield return new WaitForSeconds(2.0f);
        if (isTargetMoving == true)
        { 
            RePathFind();
        }
        isSearching = false;
    }

    private float GetTerrainHeight(Vector3 position)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPosition = terrain.transform.position;

        float normalizedX = (position.x - terrainPosition.x) / terrainData.size.x;
        float normalizedZ = (position.z - terrainPosition.z) / terrainData.size.z;

        float terrainHeight = terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);
        return terrainHeight + terrainPosition.y;
    }
}
