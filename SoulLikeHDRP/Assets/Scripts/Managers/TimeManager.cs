using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : GSingleton<TimeManager>
{
    public float gameDeltaTime = default;
    public float playerTime = default;  //플레이어의 행동 관련 속도에 deltaTime 대신 곱하려고 만든 변수
    public float monsterTime = default; //몬스터의 행동 관련 속도에 deltaTime 대신 곱하려고 만든 변수

    private float gameSpeed = default;   //Time Scale을 조정하지 않고도 게임 속도를 조절하기 위해 만든 변수
    private float playerSpeed = default; // 게임의 속도만이 아니라 플레이어의 속도만을 조절하기 위해서 만든 변수
    private float monsterSpreed = default;   // 게임의 속도만이 아니라 몬스터의 속도만을 조절하기 위해서 만든 변수 


    protected override void Init()
    {
        base.Init();
        //게임이 시작할때 게임의 속도를 기본값인 1로 맞춘다.
        gameSpeed = 1;
        playerSpeed = 1;
        monsterSpreed = 1;
    }
    //! 프레임이 밀리거나 이슈가 있을 경우를 대비해기 위해서 fixedUpdate에서 처리한다.
    protected override void FixedUpdate()
    {
        gameDeltaTime = Time.deltaTime;
        playerTime = Time.deltaTime * playerSpeed * gameSpeed;
        monsterTime = Time.deltaTime * monsterSpreed * gameSpeed;
    }

    #region TimeController
    //! 매니저에서 이 메소드를 호출하면 게임의 속도를 조절할수 있다.
    public void SetGameSpeed(float speed)
    {
        gameSpeed = speed;
    }
    //! 매니저에서 이 메소드를 호출하면 플레이어의 속도를 조절할수 있다.
    public void SetPlayerSpeed(float speed)
    {
        playerSpeed = speed;
    }   
    //! 매니저에서 이 메소드를 호출하면 몬스터의 속도를 조절할수 있다.
    public void SetMonsterSpeed(float speed)
    {
        monsterSpreed = speed;
    }
    #endregion

}
