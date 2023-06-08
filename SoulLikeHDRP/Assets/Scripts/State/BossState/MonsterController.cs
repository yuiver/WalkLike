using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    public GameObject IsMonster = default;  // 몬스터
    public GameObject targetObj = default;  // 몬스터가 추적할 대상 = 플레이어
    public Terrain terrain = default;   // 몬스터가 현재 있는 터레인을 기준으로 가져오게 된다. 아직 다른 터레인을 추적하는걸 구현하지 못했다.

    public GameObject pivot;    //몬스터의 중심점이 지면이 아니기 때문에 높이 체크에서 생기는 오류를 해소하기 위해서 만든 오브젝트다.

    public float speed = 50f;   //몬스터가 이동할 속도

    private Queue<Vector3> pathPoints;

    // Start is called before the first frame update
    void Start()
    {
        StartMove();
    }
    void StartMove()
    {
        pathPoints = new Queue<Vector3>();

        float findobjsTerrainHeight = terrain.SampleHeight(IsMonster.transform.position);
        IsMonster.transform.position = new Vector3(IsMonster.transform.position.x, findobjsTerrainHeight, IsMonster.transform.position.z);

        float targetObjTerrainHeight = terrain.SampleHeight(targetObj.transform.position);
        targetObj.transform.position = new Vector3(targetObj.transform.position.x, targetObjTerrainHeight, targetObj.transform.position.z);

        //findobjs = this.gameObject;
        AStarManager.Instance.StartPathFinding(IsMonster, targetObj, terrain);
    }


    public void SetPath(List<Vector3> path)
    {
        pathPoints.Clear();
        foreach (var point in path)
        {
            pathPoints.Enqueue(point);
        }
        Debug.Log($"Path Points: {string.Join(", ", pathPoints)}");
        StartCoroutine(FollowPath());
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
            transform.LookAt(target);

            Debug.Log($"Current Position: {transform.position}, Target Position: {target}, Distance: {Vector3.Distance(transform.position, target)}");


            if (Vector3.Distance(pivot.transform.position, target) < 0.1f)
            {
                pathPoints.Dequeue();
            }
            yield return null;
        }
    }

    private float GetTerrainHeight(Vector3 position)
    {
        Terrain terrain = Terrain.activeTerrain;
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPosition = terrain.transform.position;

        float normalizedX = (position.x - terrainPosition.x) / terrainData.size.x;
        float normalizedZ = (position.z - terrainPosition.z) / terrainData.size.z;

        float terrainHeight = terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);
        return terrainHeight + terrainPosition.y;
    }
}
