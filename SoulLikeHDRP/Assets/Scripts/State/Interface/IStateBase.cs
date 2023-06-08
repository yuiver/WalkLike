using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStateBase
{
    void Enter();
    void Execute();
    void Exit();

}
