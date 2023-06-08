using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TerrainTools;

public class THandler : MonoBehaviour
{
    [FoldoutGroup("Map Controller")]
    public WorldController controller = default;
    [FoldoutGroup("Map Controller")]
    public bool destroyOn = false;

    [FoldoutGroup("Terrain Info")]
    public Terrain thisTerrain = default;
    [FoldoutGroup("Terrain Info")]
    public string terrainKey = default;
    [FoldoutGroup("Terrain Info")]
    public Vector2 playerPosition = default;
    [FoldoutGroup("Terrainap Info")]
    public int keyX = default;
    [FoldoutGroup("Terrain Info")]
    public int keyZ = default;
    [FoldoutGroup("Terrain Info")]
    public bool playerOn = false;

    [FoldoutGroup("Direction Check Bool")]
    public float delay = default;
    [FoldoutGroup("Direction Check Bool")]
    public bool eastLoad = false;
    [FoldoutGroup("Direction Check Bool")]
    public bool westLoad = false;
    [FoldoutGroup("Direction Check Bool")]
    public bool southLoad = false;
    [FoldoutGroup("Direction Check Bool")]
    public bool northLoad = false;
    [FoldoutGroup("Direction Check Bool")]
    public bool southEastLoad = false;
    [FoldoutGroup("Direction Check Bool")]
    public bool southWestLoad = false;
    [FoldoutGroup("Direction Check Bool")]
    public bool northEastLoad = false;
    [FoldoutGroup("Direction Check Bool")]
    public bool northWestLoad = false;



    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (playerOn == false)
            {
                playerOn = true;
                GFunc.Log($"플레이어는 {terrainKey}에 입장했습니다.");
            }
        }
    }

    void Start()
    {
        ResetDirectionBool();
        controller = this.transform.parent.GetComponent<WorldController>();
        thisTerrain = this.transform.GetComponent<Terrain>();
        keyX = (int)(this.transform.localPosition.x * 0.001);
        keyZ = (int)(this.transform.localPosition.z * 0.001);
        terrainKey = $"x{keyX}y{keyZ}";
        //첫 맵을 로드할때 키에 추가해주기 위한 if문, 혹시 캐싱이 안되는 일이 생겼을때에 대비해 2중으로 체크한다.
        if (!controller.LoadterrainKeyList.Contains(terrainKey))
        {
            controller.LoadterrainKeyList.Add(terrainKey);
        }
    }

    Vector3 PlayerTerrainPosition = default;

    void Update()
    {
        delay += Time.deltaTime;
        //! 맵을 벗어났을때 맵을 destroy 하기위해 맵 기준의 플레이어 좌표를 구해야하는데 오버헤드가 크게 발생하는걸 방지 하기 위해서 맵을 나갈경우에는 딜레이를 주고 거리를 체크하게 만들었다.
        if (playerOn == true || delay > 5)
        {
            PlayerTerrainPosition = GetPlayerTerrainPosition(GameManager.Instance.playerController.transform.position);
            delay= 0;
        }
        //! 맵을 벗어났을때 일정 거리 이상이 된다면 맵을 파괴하는 함수
        if (PlayerTerrainPosition.x < -400 || PlayerTerrainPosition.x > 1400 || PlayerTerrainPosition.z < -400 || PlayerTerrainPosition.z > 1400)
        {
            if (destroyOn == false)
            {
                destroyOn = true;
                controller.LoadterrainKeyList.Remove(terrainKey);
                Destroy(this.gameObject, 5.0f);
            }
        }

        if (GameManager.Instance.playerController != null && playerOn == true)
        {
            //Debug.Log($"Player's terrain position:{PlayerTerrainPosition}");
            if (PlayerTerrainPosition.x < 0 || PlayerTerrainPosition.x > 1000 || PlayerTerrainPosition.z < 0 || PlayerTerrainPosition.z > 1000)
            {
                playerOn = false;
                Debug.Log($"플레이어가 {terrainKey}터레인을 나갔습니다.");
            }
            else
            {
                if (PlayerTerrainPosition.x > 750) //&& eastLoad == false
                {
                    eastLoad = true;
                    WorldMapLoad(Direction.East);
                }
                if (PlayerTerrainPosition.x < 250) //&& westLoad == false
                {
                    westLoad = true;
                    WorldMapLoad(Direction.West);
                }
                if (PlayerTerrainPosition.z < 250) //&& southLoad == false
                {
                    southLoad = true;
                    WorldMapLoad(Direction.South);
                }
                if (PlayerTerrainPosition.z > 750) //&& northLoad == false
                {
                    northLoad = true;
                    WorldMapLoad(Direction.North);
                }
                if (PlayerTerrainPosition.x > 750 && PlayerTerrainPosition.z < 250) //  && southEastLoad == false
                {
                    southEastLoad = true;
                    WorldMapLoad(Direction.SouthEast);
                }
                if (PlayerTerrainPosition.x < 250 && PlayerTerrainPosition.z < 250) //  && southWestLoad == false
                { 
                    southWestLoad = true;
                    WorldMapLoad(Direction.SouthWest);
                }
                if (PlayerTerrainPosition.x > 750 && PlayerTerrainPosition.z > 750) //&& northEastLoad == false
                {
                    northEastLoad = true;
                    WorldMapLoad(Direction.NorthEast);
                }
                if (PlayerTerrainPosition.x < 250 && PlayerTerrainPosition.z > 750) //&& northWestLoad == false
                { 
                    northEastLoad = true;
                    WorldMapLoad(Direction.NorthWest);
                }
            }

        }
    }
    // 안정성을 위해 start 타이밍에 한번 여러번 로드를 방지하는 Bool값을 초기화 해주는 함수 가독성을 위해 캡슐화했다.
    private void ResetDirectionBool()
    {
        destroyOn = false;

        eastLoad = false;
        westLoad = false;
        southLoad = false;
        northLoad = false;
        southEastLoad = false;
        southWestLoad = false;
        northEastLoad = false;
        northWestLoad = false;
    }
    //! 원하는 방향의 맵을 로드하는 함수 스위치가 스트링이면 굉장히 무겁다 시발 하지마라 바꿔라 이넘 비교해라
    private void WorldMapLoad(Direction type)
    {
        switch (type)
        {
            case Direction.East:
                KeyCheckAndLoad(keyX + 1, keyZ);
                break;
            case Direction.West:
                KeyCheckAndLoad(keyX -1, keyZ);
                break;
            case Direction.South:
                KeyCheckAndLoad(keyX, keyZ -1);
                break;
            case Direction.North:
                KeyCheckAndLoad(keyX, keyZ +1);
                break;
            case Direction.SouthEast:
                KeyCheckAndLoad(keyX + 1, keyZ -1);
                break;
            case Direction.SouthWest:
                KeyCheckAndLoad(keyX - 1, keyZ - 1);
                break;
            case Direction.NorthEast:
                KeyCheckAndLoad(keyX + 1, keyZ + 1);
                break;
            case Direction.NorthWest:
                KeyCheckAndLoad(keyX - 1, keyZ + 1);
                break;
            default: 
                break;
        }
    }
    //! 플레이어의 터레인상의 좌표를 1000x1000 기준으로 구해서 리턴
    private Vector3 GetPlayerTerrainPosition(Vector3 playerPosition)
    {
        float terrainHeight = thisTerrain.SampleHeight(playerPosition);
        Vector3 terrainPositionWorld = new Vector3(playerPosition.x, terrainHeight, playerPosition.z);
        Vector3 terrainPositionLocal = thisTerrain.transform.worldToLocalMatrix.MultiplyPoint3x4(terrainPositionWorld);
        return terrainPositionLocal;
    }
    //! 로드하고 싶은 터레인의 좌표를 입력해서 로드하는 방식 8가지의 경우의 수가 있기 때문에 그냥 int를 매개변수로 받는 방식으로 구현
    private void KeyCheckAndLoad(int terrainX , int terrainZ)
    {
        // 터레인의 범위를 넘어갔는지 체크한다. 0,3을 나중에 맵이 추가되거나 한다면 const로 정의해서 사용하는것도 좋을것이다.
        if (terrainX < 0 || terrainX > 3 || terrainZ < 0 || terrainZ > 3) { return; }

        // key값으로 리소스를 로드하기 때문에 로드할 위치를 string으로 만들어준다.
        string loadKey = $"x{terrainX}y{terrainZ}";

        // 이미 로드된 key값이라면 로드하지 못하게 하는 조건이 걸린 if문
        if (!controller.LoadterrainKeyList.Contains(loadKey))
        {
            controller.LoadterrainKeyList.Add(loadKey);
            ResourceManager.Instance.Instantiate(loadKey, gameObject.transform.parent);
        }
    }
}
