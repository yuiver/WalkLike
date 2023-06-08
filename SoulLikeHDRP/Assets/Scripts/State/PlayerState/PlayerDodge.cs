using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDodge : IStateBase
{
    private PlayerController _playerController;
    private Animator _animator;

    public PlayerDodge(PlayerController playerController)
    {
        _playerController = playerController;
        _animator = _playerController.GetComponent<Animator>();
    }
    public void Enter()
    {
        _animator.SetTrigger("Dodge");
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
