using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    // 오직 키액션 델리게이트를 받기 위한 인풋 관리 싱글톤
public class InputManager : GSingleton<InputManager>
{
    public Action KeyAction = null;

    protected override void Update()
    {
        base.Update();
        if (Input.anyKey == false)
        {
            return;
        } 
        if (KeyAction != null)
        { 
            KeyAction.Invoke();
        }
    }
}
