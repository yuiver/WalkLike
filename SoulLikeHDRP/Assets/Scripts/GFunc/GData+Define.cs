using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static partial class GData 
{


}

//! 맵의 방위를 정의하기 위한 코드 터레인을 실시간으로 로드하기위해 추가했는데 터레인 사이즈가 너무 커서 효율이 떨어지므로 터레인의 사이즈를 줄이는 과정이 필요하다.
public enum Direction
{
    Unknown,
    East,
    West,
    South,
    North,
    SouthEast,
    SouthWest,
    NorthEast,
    NorthWest,
}


//! 지형의 속성을 정의하기 위한 타입 이 타입도 A*알고리즘의 동작 테스트를 위해 사용하던 코드입니다.
public enum TerrainType
{ 
    NONE = -1, 
    PLAIN_PASS,
    OCEAN_N_PASS
}       // TerrainType


/// [Yuiver] 2023-03-16
/// @brief 사운드의 속성 분류를 위해 반복해서 재생하는 BGM과 이펙트사운드의 타입을 따로 정의했다.
/// @Stats MaxCount 매직넘버를 피하기 위해서 만들었고, AudioSource[]를 만들때 들어가는 배열의 길이를 정의하기 위해 enum타입의 마지막에 넣었다.

//! 사운드의 속성을 정의하기 위한 타입
public enum Sound
{
    Bgm,        /// < Sound BackGround Audio Loop
    SFX,        /// < Sound Effect Audio Play Once
    UI_SFX,     /// < Sound UI Effect Audio Play Once
    MaxCount,   /// < Sound AudioSource Length
}
//! 플레이어의 상태를 정의하기 위한 타입
public enum PlayerState
{ 
    Idle,
    Move,
    Run,
    Attack,
    GetHit,
    Death,
    MaxCount,
}

public enum SceneType
{
    Unknown,
    TestInit,
    TitleScene,
    GameScene,
}