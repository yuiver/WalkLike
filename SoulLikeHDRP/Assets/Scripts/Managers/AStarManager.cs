using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarManager : GSingleton<AStarManager>
{

    protected override void Init()
    {
        base.Init();
    }

    private Vector3 startPoint = default;
    private Vector3 endPoint = default;
    private int[] mapSize = default;
    public HashSet<Vector3> obstacles = new HashSet<Vector3>();

    // 장애물 추가 메서드
    public void AddObstacle(GameObject obstacle, float radius)
    {
        Vector3 obstaclePosition = obstacle.transform.position;
        for (int x = -Mathf.RoundToInt(radius); x <= Mathf.RoundToInt(radius); x++)
        {
            for (int z = -Mathf.RoundToInt(radius); z <= Mathf.RoundToInt(radius); z++)
            {
                Vector3 newPosition = new Vector3(obstaclePosition.x + x, obstaclePosition.y, obstaclePosition.z + z);
                Debug.Log($"장애물의 위치는 {newPosition}");
                obstacles.Add(newPosition);
            }
        }
    }

    //! 이 함수는 startPointObj와 endPointObj사이의 길을 A* 알고리즘을 사용해서 찾습니다.
    public void StartPathFinding(GameObject startPointObj, GameObject endPointObj, Terrain terrain)
    {
        // 시작과 끝 지점의 포지션을 얻는다.
        startPoint = startPointObj.transform.position;
        endPoint = endPointObj.transform.position;

        // 정수 좌표로 변환
        startPoint = new Vector3(Mathf.RoundToInt(startPoint.x), startPoint.y, Mathf.RoundToInt(startPoint.z));
        endPoint = new Vector3(Mathf.RoundToInt(endPoint.x), endPoint.y, Mathf.RoundToInt(endPoint.z));

        // Terrain의 크기를 얻는다.
        mapSize = new int[] { (int)terrain.terrainData.size.x, (int)terrain.terrainData.size.z };

        // 객체를 생성하고 경로를 찾습니다.
        AStar3DAlgo pathFinder = new AStar3DAlgo(startPointObj, startPoint, endPoint, terrain, mapSize, obstacles);
        //List<Vector3> path = pathFinder.PathFind();

        // 코루틴을 시작하고, 경로가 완료되면 오브젝트를 이동시킵니다.
        StartCoroutine(pathFinder.PathFindCoroutine((path) => {
            enemyController enemyController = startPointObj.GetComponent<enemyController>();
            if (enemyController != null)
            {
                enemyController.SetPath(path);
            }

            // 경로를 콘솔에 출력합니다.
            foreach (Vector3 point in path)
            {
                Debug.Log(point);
            }
        }));
    }

}
// AStar3DAlgo 클래스는 3차원 지형에서 A* 알고리즘을 이용해서 경로를 찾는 클래스
public class AStar3DAlgo
{
    private GameObject _startPointObj = default;
    private int[] _mapSize = default;
    private Vector3 _startPoint = default;
    private Vector3 _endPoint = default;
    private Terrain _terrain;
    private HashSet<Vector3> _obstacles;

    private float minDistanceToTarget = 5.0f; // 목표 지점과의 최소 거리 설정

    // 생성자에서는 시작점, 끝점, 지형 ,맵 크기 를 입력받는다.
    public AStar3DAlgo(GameObject startObj, Vector3 startPoint, Vector3 endPoint, Terrain terrain, int[] mapSize, HashSet<Vector3> obstacles)
    {
        _startPointObj = startObj;
        _mapSize = mapSize;
        _startPoint = startPoint;
        _endPoint = endPoint;
        _terrain = terrain;
        _obstacles = obstacles;

        Debug.Log($"Start Point: {_startPoint}, End Point: {_endPoint}, Map Size: {_mapSize[0]}x{_mapSize[1]}");
    }

    // 해당 위치에 장애물이 없는지 확인하는 메서드
    private bool IsWalkable(Vector3 position)
    {
        return !_obstacles.Contains(position);
    }
    //! 이 함수는 현재 위치에서 다음 위치로 이동하는 비용을 계산한다.
    private float GetCost(Vector3 current, Vector3 next)
    {
        float currentHeight = _terrain.SampleHeight(current);
        float nextHeight = _terrain.SampleHeight(next);
        float heightDifference = Mathf.Abs(currentHeight - nextHeight);
        float slopeFactor = Mathf.Max(heightDifference, 1); // 경사도에 따른 가중치를 추가합니다. 이 값을 조정하여 경사에 민감하게 반응하도록 할 수 있습니다.
        //Debug.Log($"Cost from {current} to {next}: {Vector3.Distance(current, next) * slopeFactor}");
        return Vector3.Distance(current, next) * slopeFactor; // 경사에 따른 가중치를 거리에 곱하여 비용을 계산합니다.
    }
    // 이 함수는 현재 위치와 목표 위치 사이의 거리를 계산한다.
    private float GetDistance(Vector3 current, Vector3 end)
    {
        Debug.Log($"Heuristic distance from {current} to {end}: {Vector3.Distance(current, end)}");
        return Vector3.Distance(current, end);
    }

    private float minDistanceFromObstacles = 1.0f; // 대상과 장애물 사이의 최소 거리를 지정합니다.

    // 이 함수는 주어진 위치 주변의 붙어있는 위치를 반환한다.
    private List<Vector3> GetNeighbors(Vector3 current)
    {
        List<Vector3> neighbors = new List<Vector3>();

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0)
                {
                    continue;
                }
                int newX = Mathf.RoundToInt(current.x) + x;
                int newZ = Mathf.RoundToInt(current.z) + z;

                // 해당 위치가 맵 범위 내에 있는지 확인
                if (newX >= 0 && newX < _mapSize[0] && newZ >= 0 && newZ < _mapSize[1])
                {   // 이웃 위치를 구한 다음 높이를 지형 데이터에서 샘플링한 값으로 설정
                    float newY = _terrain.SampleHeight(new Vector3(newX, 0, newZ));
                    Vector3 newPosition = new Vector3(newX, newY, newZ);

                    // 장애물과의 거리를 확인하고 충분한 거리가 있는 경우에만 이웃 목록에 추가
                    if (!IsTooCloseToObstacles(newPosition))
                    {
                        neighbors.Add(newPosition);
                    }
                }
            }
        }
        //Debug.Log($"Neighbors of {current}: {string.Join(", ", neighbors)}");
        return neighbors;
    }
    // 주어진 위치가 장애물과 너무 가까운지 확인하는 메서드
    private bool IsTooCloseToObstacles(Vector3 position)
    {
        foreach (Vector3 obstacle in AStarManager.Instance.obstacles)
        {
            if (Vector3.Distance(position, obstacle) < minDistanceFromObstacles)
            {
                return true;
            }
        }
        return false;
    }

    //! A* 알고리즘을 사용하여 경로를 찾는 함수
    public IEnumerator PathFindCoroutine(Action<List<Vector3>> onPathFound)
    {
        // cameFrom: 어떤 위치로부터 왔는지를 저장
        Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>();
        // costSoFar: 시작점으로부터 특정 위치까지의 비용을 저장
        Dictionary<Vector3, float> costSoFar = new Dictionary<Vector3, float>();
        // 제대로 탐색중인지 확인 하기 위해서 만든 변수


        // 우선 순위 큐를 사용하여 탐색할 위치를 저장
        var frontier = new PriorityQueue<Vector3>(_mapSize[0] * _mapSize[1]);
        frontier.Enqueue(_startPoint, 0);

        // 시작점의 cameFrom과 costSoFar를 설정
        cameFrom[_startPoint] = _startPoint;
        costSoFar[_startPoint] = 0;

        const int maxIterations = 10000; // 최대 반복 횟수 설정
        int iterations = 0;

        // 우선 순위 큐에 위치가 남아있는 동안 계속 반복
        while (frontier.Count > 0)
        {
            iterations++;
            if (iterations > maxIterations)
            {
                //Debug.LogWarning("A* Algorithm reached the maximum number of iterations.");
                break;
            }

            Debug.Log($"Frontier Count: {frontier.Count}"); // 큐 크기 출력
            // 우선 순위 큐에서 가장 낮은 비용을 가진 위치를 가져옴
            var current = frontier.Dequeue();

            if (Vector3.Distance(current, _endPoint) <= minDistanceToTarget)
            {
                _endPoint = current;
                break;
            }

            // 현재 위치의 이웃들을 반복하면서 처리
            foreach (var next in GetNeighbors(current))
            {
                // 현재 위치에서 다음 위치로 이동하는데 드는 새로운 비용을 계산
                float newCost = costSoFar[current] + GetCost(current, next);
                // 이웃 위치가 비용 사전에 없거나 새 비용이 기존 비용보다 작으면 값을 업데이트하고 큐에 추가
                if (costSoFar.ContainsKey(next) == false || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    float priority = newCost + GetDistance(next, _endPoint);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
                //_startPointObj.transform.position = next;

                if (0 > 1 == true)
                    yield return new WaitForSeconds(0.3f);
            }
        }

        // 최종 경로를 저장할 리스트를 생성
        List<Vector3> path = new List<Vector3>();

        // 목표 지점에 도달한 경로가 없으면 빈 경로를 반환
        if (cameFrom.ContainsKey(_endPoint) == false)
        {
            Debug.Log("No path found");
            onPathFound?.Invoke(path);
        }

        // cameFrom 사전을 사용하여 경로를 거꾸로 추적
        Vector3 temp = _endPoint;
        while (temp != _startPoint)
        {
            Debug.Log($"Current Path");
            path.Add(temp);
            temp = cameFrom[temp];
        }
        // 시작점을 경로에 추가한 다음, 경로를 뒤집어 올바른 순서로 만든다.
        path.Add(_startPoint);
        path.Reverse();

        // 최종 경로를 반환
        onPathFound?.Invoke(path);
    }

}

// 우선순위 큐 노드 클래스
public class PriorityQueueNode<T>
{
    public T Item { get; set; } // 노드에 저장되는 항목
    public float Priority { get; set; } // 노드의 우선순위 값

    public PriorityQueueNode(T item, float priority)
    {
        Item = item;
        Priority = priority;
    }
}

public class PriorityQueue<T>
{
    private List<PriorityQueueNode<T>> elements; // 노드 목록

    public int Count
    {
        get { return elements.Count; } // 큐의 요소 개수 반환
    }

    // 생성자로 초기 용량 설정
    public PriorityQueue(int initialCapacity)
    {
        elements = new List<PriorityQueueNode<T>>(initialCapacity);
    }

    // 새 항목과 우선순위를 받아 큐에 추가
    public void Enqueue(T item, float priority)
    {
        var newNode = new PriorityQueueNode<T>(item, priority); // 새 노드 생성
        int currentIndex = elements.Count;
        elements.Add(newNode); // 노드 목록에 추가

        // 삽입 정렬
        while (currentIndex > 0)
        {
            int parentIndex = (currentIndex - 1) / 2;
            if (elements[currentIndex].Priority >= elements[parentIndex].Priority)
            {
                break;
            }
            Swap(currentIndex, parentIndex); // 위치 변경
            currentIndex = parentIndex; // 인덱스 업데이트
        }
    }

    // 가장 낮은 우선순위의 항목을 제거하고 반환
    public T Dequeue()
    {
        if (elements.Count == 0)
        {
            throw new InvalidOperationException("The queue is empty.");
        }
        T item = elements[0].Item;
        int lastIndex = elements.Count - 1;
        elements[0] = elements[lastIndex]; // 마지막 노드를 루트로 이동
        elements.RemoveAt(lastIndex); // 마지막 노드 제거

        int currentIndex = 0;
        // 힙 정렬
        while (true)
        {
            int leftChildIndex = 2 * currentIndex + 1;
            int rightChildIndex = 2 * currentIndex + 2;
            int minIndex = currentIndex;

            // 왼쪽 자식이 현재 노드보다 작은 경우
            if (leftChildIndex < lastIndex && elements[leftChildIndex].Priority < elements[minIndex].Priority)
            {
                minIndex = leftChildIndex;
            }
            // 오른쪽 자식이 현재 노드보다 작은 경우
            if (rightChildIndex < lastIndex && elements[rightChildIndex].Priority < elements[minIndex].Priority)
            {
                minIndex = rightChildIndex;
            }

            // 자식 노드보다 작으면 정렬 완료
            if (minIndex == currentIndex)
            {
                break;
            }

            Swap(currentIndex, minIndex); // 위치 변경
            currentIndex = minIndex; // 인덱스 업데이트
        }

        return item;
    }

    // 두 인덱스의 노드 위치를 바꾸는 메서드
    private void Swap(int indexA, int indexB)
    {
        var temp = elements[indexA]; // 임시 변수에 첫 번째 인덱스의 요소 저장
        elements[indexA] = elements[indexB]; // 첫 번째 인덱스에 두 번째 인덱스의 요소 할당
        elements[indexB] = temp; // 두 번째 인덱스에 임시 변수에 저장된 요소 할당
    }
}
