using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 이 코드를 컴포넌트로 가지고 있는 UI가 동작하지 않는다면 이 UI를 이용해서 열고 닫는
/// 게임오브젝트를 인스펙터에서 지정해 줘야하는데 그것이 빠져있을 가능성이 있으니 체크하십시오.
/// </summary>
public class TitleButtonController : MonoBehaviour
{

    //버튼 클릭을 받는 컴포넌트의 리스트를 만들어서 자식으로 가지고 있는 버튼을 전부 캐싱하기 위한 리스트.
    [SerializeField]
    List<TitleButtonHandler> buttonHandlers = default;

    //키입력과 마우스입력을 둘다 받기 위해서 만든 인덱스 넘버 선택된 인덱스 값으로 실행함
    private int buttonIndex = -1;


    //어떤 메뉴를 SetActive(true) 할것인지 파싱하기 위한 변수
    public GameObject optionMenu = default;
    //옵션창이 켜져있으면 예외처리를 통해 키입력을 무시하기 위한 변수
    public static bool optionActive = false;
    //세이브 데이터에 Continue가능한 데이터가 존재할 경우 Continue버튼을 활성화 시키기 위해 만든 변수
    public static bool hasValidSaveData = false;

    //! Awake에서는 세이브 데이터를 체크하고 팝업 UI의 활성화 여부를 파악하는 bool변수를 초기화합니다.
    private void Awake()
    {
        //스타트에서 호출하니 참조 오류가 떳다.
    }

    //! 스타트에서는 자식으로 가지고 있는 버튼의 핸들러를 모두 리스트로 가져옵니다. 컨티뉴 버튼을 활성화 시켜둘지 결정하고,
    //인덱스 값을 한번 초기값으로 초기화하고 초기값에 해당하는 버튼을 활성화 색으로 변경합니다.
    private void Start()
    {
        optionActive = false;
        //이 부분에서 오브젝트 자식의 버튼핸들러를 모두 리스트로 저장합니다.
        //LINQ를 사용하는게 이슈가 될수도 있습니다.
        //IOS에서 문제가 된다면 이 코드를 수정하는걸 권장합니다.
        buttonHandlers = transform.GetComponentsInChildren<TitleButtonHandler>().ToList();
        for (int i = 0; i < buttonHandlers.Count; i++)
        {
            buttonHandlers[i].titleButtonController = this;
            buttonHandlers[i].buttonIndex = i;
        }


        //이 부분에서 버튼의 인덱스값을 초가화하고 버튼색을 바꿔줍니다.
        buttonIndex = 0;
        ActivateSingleButton(buttonIndex);
    }


    private void Update()
    {
        // 옵션메뉴가 켜져있다면 타이틀 메뉴는 키보드로 동작하지 않는다.
        if (optionActive == false)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ActivateSingleButton(LimitKeyBoardIndex(buttonIndex + (-1)));
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ActivateSingleButton(LimitKeyBoardIndex(buttonIndex + 1));
            }
            //선택키를 누를때 인덱스값을 결정하는 스위치는 핸들러와 컨트롤러가 따로 가지고 있기 때문에
            //이 코드를 수정한다면 핸들러의 수정도 필요합니다.
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z))
            {
                switch (buttonIndex)
                {
                    case 0:
                        //Debug.Log("게임 플레이 씬 이름을 넣어주세요. 아직 설정하지 않았습니다.");
                        SceneManagerEX.Instance.ChangeScene(SceneType.GameScene);
                        break;
                    case 1:
                        OptionMenuActive();
                        break;
                    case 2:
                        GFunc.QuitThisGame();
                        break;
                    default:
                        break;
                }

            }
        }

    }
    //! 위에 빈 GameObject변수에 넣어준 게임오브젝트를 DOTween함수를 사용해서 코루틴으로 SetActive(true)로 만들어주는 함수
    public void OptionMenuActive()
    {
        StartCoroutine(OptionScaleActive());
    }
    //! DOTween함수를 이용해서 오브젝트를 활성화와 동시에 Ease효과를 줍니다.
    IEnumerator OptionScaleActive()
    {
        optionActive = true;
        optionMenu.SetActive(true);
        optionMenu.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetAutoKill();
        yield return new WaitForSeconds(0.3f);
    }

    //! 하나의 선택된 인덱스에 해당하는 버튼만 색을 바꾸라고 핸들러에 명령을 전달하는 함수
    public void ActivateSingleButton(int index)
    {
        if (buttonHandlers.IsValid() == false)
        {
            buttonIndex = -1;
        }
        else
        {
            for (int i = 0; i < buttonHandlers.Count; i++)
            {
                buttonHandlers[i].ButtonSelect(i == index);

                if (i == index)
                {
                    buttonIndex = index;
                }
            }
        }
    }
    //! 키보드로 UI를 조작할때 인덱스의 값이 배열의 최소,최대값보다 작아지거나 커지는것을 방지하는 함수
    private int LimitKeyBoardIndex(int index_)
    {
        int resultIndex = index_;
        if (index_ >= buttonHandlers.Count)
        {
            resultIndex = 0;
        }
        else if (index_ < 0)
        {
            resultIndex = buttonHandlers.Count - 1;
        }

        return resultIndex;
    }
}