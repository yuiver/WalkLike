using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : IStateBase
{
    private PlayerController _playerController;
    private Animator _animator;

    public PlayerAttack(PlayerController playerController)
    {
        _playerController = playerController;
        _animator = _playerController.GetComponent<Animator>();
    }
    public void Enter()
    {
        _animator.SetTrigger("Attack");
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
