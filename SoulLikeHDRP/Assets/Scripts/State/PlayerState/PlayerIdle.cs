using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//! 아이들 체크를 스테이트로 받을 경우에 생기는 문제에 대응해서 플레이어 코드로 관련 조직을 옮겼습니다.
public class PlayerIdle : IStateBase
{
    private PlayerController _playerController;
    private Animator _animator;

    public PlayerIdle(PlayerController playerController)
    {
        //_playerController = playerController;
        //_animator = _playerController.GetComponent<Animator>();
    }
    public void Enter()
    {
        //Debug.Log("아이들 엔터");
    }

    public void Execute()
    {
        // 아무 것도 하지 않음
    }

    public void Exit()
    {
        // 아무 것도 하지 않음
    }
}
