using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//A스타 노트테스트를 위해 만들어져있는 코드입니다.
public static class RDefine
{
    public const string TERRAIN_PREF_OCEAN = "Terrain_Ocean";
    public const string TERRAIN_PREF_PLAIN = "Terrain_Plain";

    public const string OBSTACLE_PREF_PLAIN_CASTLE = "Obstacle_PlainCastle";

    public enum TileStatusColor
    { 
        DEFAULT, SELECTED, SEARCH, INACTIVE
    }
}
