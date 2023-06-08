using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

public class InGameCanvasController : MonoBehaviour
{
    //버튼 클릭을 받는 컴포넌트의 리스트를 만들어서 자식으로 가지고 있는 버튼을 전부 캐싱하기 위한 리스트.
    [SerializeField]
    private List<StopUIButtonHandler>StopUIButtonHandlers = default;
    [SerializeField] 
    private List<InGameOptionButtonHandler> optionUIButtonHandlers = default;

    //키입력과 마우스입력을 둘다 받기 위해서 만든 인덱스 넘버 선택된 인덱스 값으로 실행함
    private int StopUIbuttonIndex = -1;
    private int optionUIbuttonIndex = -1;

    //어떤 메뉴를 SetActive(true) 할것인지 파싱하기 위한 변수
    public GameObject optionMenu = default;
    //옵션창이 켜져있으면 예외처리를 통해 키입력을 무시하기 위한 변수
    public static bool optionActive = false;

    //플레이어의 현재 상태를 표기하는 UI Image를 받기 위한 변수
    public Image playerHp = default;
    public Image playerSP = default;
    public Image playerSTA = default;


    private bool setCamera = false;

    private Volume volume = default;
    private RayTracingSettings rayTracingSettings;
    private GameObject inGameUI = default;

    private Image UIpanel = default;
    private Canvas inGameUICanvas = default;
    private CanvasScaler inGameUIScaler = default;
    private bool inGameUIOpen = false;
    // Start is called before the first frame update
    private void Start()
    {
        //volume = gameObject.GetComponent<Volume>();
        //volume.profile.TryGet<RayTracingSettings>(out rayTracingSettings);
        //volume.transform.GetComponent<RayTracingSettings>();
        UIpanel = this.GetComponent<Image>();
        UIpanel.enabled = false;
        GameManager.Instance.isGameStop = false;
        inGameUICanvas = this.gameObject.GetComponent<Canvas>();
        inGameUIScaler = this.gameObject.GetComponent<CanvasScaler>();
        inGameUI = gameObject.transform.GetChild(1).gameObject; //시작할때 비활성화 해야하는 UI는 UI오브젝트이다.

        //이 부분에서 오브젝트 자식의 버튼핸들러를 모두 리스트로 저장합니다.
        //LINQ를 사용하는게 이슈가 될수도 있습니다.
        //IOS에서 문제가 된다면 이 코드를 수정하는걸 권장합니다.
        StopUIButtonHandlers = inGameUICanvas.gameObject.transform.GetComponentsInChildren<StopUIButtonHandler>().ToList();
        for (int i = 0; i < StopUIButtonHandlers.Count; i++)
        {
            StopUIButtonHandlers[i].inGameCanvasController = this;
            StopUIButtonHandlers[i].buttonIndex = i;
        }
        optionUIButtonHandlers = inGameUICanvas.gameObject.transform.GetComponentsInChildren<InGameOptionButtonHandler>().ToList();
        for (int i = 0; i < optionUIButtonHandlers.Count; i++)
        {
            optionUIButtonHandlers[i].inGameCanvasController = this;
            optionUIButtonHandlers[i].buttonIndex = i;
        }
        inGameUI.SetActive(false);
        inGameUIOpen = false;
        optionActive = false;


        //이 부분에서 버튼의 인덱스값을 초가화하고 버튼색을 바꿔줍니다.
        ReSetStopButtonIndex();
        ReSetOptionButtonIndex();
    }
    private void ReSetStopButtonIndex()
    {
        StopUIbuttonIndex = 0;
        ActivateStopUISingleButton(StopUIbuttonIndex);
    }
    private void ReSetOptionButtonIndex()
    {
        optionUIbuttonIndex = 0;
        ActivateStopUISingleButton(optionUIbuttonIndex);
    }

    // Update is called once per frame
    private void Update()
    {

        if (Camera.main != null && setCamera == false)
        {
            setCamera= true;
            inGameUICanvas.worldCamera = Camera.main;
            inGameUIScaler.referenceResolution = new Vector2(Screen.width,Screen.height); 
        }

        playerHp.fillAmount = (float)GameManager.Instance.playerHp / GameManager.Instance.playerMaxHp;
        playerSP.fillAmount = (float)GameManager.Instance.playerMana / GameManager.Instance.playerMaxMana;
        playerSTA.fillAmount = (float)GameManager.Instance.playerStamina / GameManager.Instance.playerMaxStamina;

        // 옵션메뉴가 켜져있다면 타이틀 메뉴는 키보드로 동작하지 않는다.
        if (optionActive == false)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (inGameUIOpen == false)
                {
                    inGameUIOpen = true;
                    TimeManager.Instance.SetGameSpeed(0.0f);
                    inGameUI.SetActive(true);
                    UIpanel.enabled = true;
                    GameManager.Instance.isGameStop = true;
                    ReSetStopButtonIndex();
                }
                else if (inGameUIOpen == true)
                {
                    inGameUIOpen = false;
                    TimeManager.Instance.SetGameSpeed(1.0f);
                    inGameUI.SetActive(false);
                    UIpanel.enabled = false;
                    GameManager.Instance.isGameStop = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ActivateStopUISingleButton(LimitStopUIKeyBoardIndex(StopUIbuttonIndex + (-1)));
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ActivateStopUISingleButton(LimitStopUIKeyBoardIndex(StopUIbuttonIndex + 1));
            }
            //선택키를 누를때 인덱스값을 결정하는 스위치는 핸들러와 컨트롤러가 따로 가지고 있기 때문에
            //이 코드를 수정한다면 핸들러의 수정도 필요합니다.
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z))
            {
                switch (StopUIbuttonIndex)
                {
                    case 0:
                        ContinueButton();
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
        else if (optionActive == true)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OptionMenuDisable();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ActivateOptionUISingleButton(LimitOptionUIKeyBoardIndex(optionUIbuttonIndex + (-1)));
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ActivateOptionUISingleButton(LimitOptionUIKeyBoardIndex(optionUIbuttonIndex + 1));
            }
            //선택키를 누를때 인덱스값을 결정하는 스위치는 핸들러와 컨트롤러가 따로 가지고 있기 때문에
            //이 코드를 수정한다면 핸들러의 수정도 필요합니다.
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z))
            {
                switch (optionUIbuttonIndex)
                {
                    case 0:
                        //0번 옵션 활성화
                        break;
                    case 1:
                        //1번 옵션 활성화
                        break;
                    case 2:
                        //2번 옵션 활성화
                        break;
                    default:
                        break;
                }

            }
        }
    }
    public void ContinueButton()
    {
        if (inGameUIOpen == true)
        {
            UIpanel.enabled = false;
            GameManager.Instance.isGameStop = false;
            inGameUIOpen = false;
            TimeManager.Instance.SetGameSpeed(1.0f);
            inGameUI.SetActive(false);
        }
    }
    //! 위에 빈 GameObject변수에 넣어준 게임오브젝트를 DOTween함수를 사용해서 코루틴으로 SetActive(bool)로 만들어주는 함수
    public void OptionMenuActive()
    {
        ReSetOptionButtonIndex();
        StartCoroutine(OptionScaleActive());
    }
    public void OptionMenuDisable()
    {
        StartCoroutine(OptionScaleDisable());
    }
    //! DOTween함수를 이용해서 오브젝트를 활성화와 동시에 Ease효과를 줍니다.
    IEnumerator OptionScaleActive()
    {
        optionActive = true;
        optionMenu.SetActive(true);
        optionMenu.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetAutoKill();
        yield return new WaitForSeconds(0.3f);
    }
    IEnumerator OptionScaleDisable()
    {
        optionActive = false;
        optionMenu.SetActive(false);
        optionMenu.transform.DOScale(0.8f, 0.3f).SetEase(Ease.InBack).SetAutoKill();
        yield return new WaitForSeconds(0.3f);
    }

    //! 하나의 선택된 인덱스에 해당하는 버튼만 색을 바꾸라고 핸들러에 명령을 전달하는 함수
    public void ActivateStopUISingleButton(int index)
    {
        if (StopUIButtonHandlers.IsValid() == false)
        {
            StopUIbuttonIndex = -1;
        }
        else
        {
            for (int i = 0; i < StopUIButtonHandlers.Count; i++)
            {
                StopUIButtonHandlers[i].ButtonSelect(i == index);

                if (i == index)
                {
                    StopUIbuttonIndex = index;
                }
            }
        }
    }
    //! 키보드로 UI를 조작할때 인덱스의 값이 배열의 최소,최대값보다 작아지거나 커지는것을 방지하는 함수
    private int LimitStopUIKeyBoardIndex(int index_)
    {
        int resultIndex = index_;
        if (index_ >= StopUIButtonHandlers.Count)
        {
            resultIndex = 0;
        }
        else if (index_ < 0)
        {
            resultIndex = StopUIButtonHandlers.Count - 1;
        }

        return resultIndex;
    }

        //! 하나의 선택된 인덱스에 해당하는 버튼만 색을 바꾸라고 핸들러에 명령을 전달하는 함수
    public void ActivateOptionUISingleButton(int index)
    {
        if (optionUIButtonHandlers.IsValid() == false)
        {
            optionUIbuttonIndex = -1;
        }
        else
        {
            for (int i = 0; i < optionUIButtonHandlers.Count; i++)
            {
                optionUIButtonHandlers[i].ButtonSelect(i == index);

                if (i == index)
                {
                    optionUIbuttonIndex = index;
                }
            }
        }
    }
    //! 키보드로 UI를 조작할때 인덱스의 값이 배열의 최소,최대값보다 작아지거나 커지는것을 방지하는 함수
    private int LimitOptionUIKeyBoardIndex(int index_)
    {
        int resultIndex = index_;
        if (index_ >= optionUIButtonHandlers.Count)
        {
            resultIndex = 0;
        }
        else if (index_ < 0)
        {
            resultIndex = optionUIButtonHandlers.Count - 1;
        }

        return resultIndex;
    }
}
