using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InGameOptionButtonHandler : BaseButtonHandler, IPointerEnterHandler, IPointerClickHandler
{

    public InGameCanvasController inGameCanvasController = default;
    public void OnPointerEnter(PointerEventData eventData)
    {
        //마우스가 올라갔을때 마우스가 올라간 버튼을 활성화하고 인덱스를 바꾸는 코드
        inGameCanvasController.ActivateOptionUISingleButton(buttonIndex);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        //버튼에 해당하는 이미지를 누를때 인덱스에 맞는 코드를 실행하는 스위치 입니다.
        //스위치는 핸들러와 컨트롤러가 따로 가지고 있기 때문에 이 코드를 수정한다면 컨트롤러의 수정도 필요합니다.
        switch (buttonIndex)
        {
            case 0:
                //?
                break;
            case 1:
                //?
                break;
            case 2:
            //?
            default:
                break;
        }
    }

    void Awake()
    {
        // 오브젝트가 활성화 됬을때 버튼에 해당하는 텍스트의 색을 변경하기 위해 버튼을 모아둔 오브젝트의 자식에서 컴포넌트를 가져온다.
        buttonText = this.transform.GetChild(0).GetComponent<TMP_Text>();
    }
}
