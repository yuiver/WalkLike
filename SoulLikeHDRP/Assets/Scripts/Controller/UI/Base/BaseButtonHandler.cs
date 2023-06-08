using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

//키보드와 마우스에 모두 대응하는 버튼 핸들러의 베이스 클래스
public class BaseButtonHandler : MonoBehaviour
{
    public static Color OnButtonColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);  ///버튼이 켜질때의 컬러
    public static Color OffButtonColor = new Color(0.5f, 0.5f, 0.5f, 1.0f); ///버튼이 꺼질때의 컬러

    protected TMP_Text buttonText = null; ///버튼에 있는 텍스트를 담아두는 변수
    public int buttonIndex = -1;

    //! 활성화 상태의 버튼이 어떤것인지 확인하기 위해서 색을 변경하는 함수
    public void ButtonSelect(bool isOn_)
    {
        if (isOn_)
        {
            buttonText.color = OnButtonColor;
        }
        else
        {
            buttonText.color = OffButtonColor;
        }
    }
}
