using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator PlayerAnimator;
    private Rigidbody rb;
    private IStateBase currentState = default;

    private bool isRuning = false;
    private bool isMoving = false;
    private bool OnGround = false;
    private bool onGetHit = false;
    private bool doingAttack = false;
    private bool canSecondAttack = false;
    private bool doingSecondAttack = false;

    private float speedMultiplier = default;
    private float RunDelay = 0f; //뛸때 스테미너 소비에 딜레이를 준다.
    private float dodgeDelay = 0f;
    private float groundDelay = 0f;
    private float groundCheckDelay = 0.2f;
    private float getHitCheckDelay = 0.2f;
    private Coroutine groundCheckCoroutine;
    private Coroutine getHitCheckCoroutine;

    public Action<string,bool> SetAniMatorBool;
    public Camera mainCamera = default;

    public THandler currentTerrain = default; 
    //! 플레이어의 정보를 캐싱하기 위해서 Awake에서 게임매니저에 플레이어 컨트롤러를 캐싱

    private void Start()
    {
        //mainCamera = GameManager.Instance.cameraController.gameObject.GetComponent<Camera>();
        SetAniMatorBool = default;
        SetAniMatorBool += (animatorBoolName, boolState) => PlayerAnimator.SetBool(animatorBoolName, boolState);

        OnGround =true; // 시작할때 땅에 닿아있다고 판단
        rb = GetComponent<Rigidbody>();
        PlayerAnimator = GetComponent<Animator>();

        SetState(new PlayerIdle(this));   //플레이어의 상태가 아이들인지 체크하기 위한 코드였는데 레거시상태

        //인풋매니저가 키보드 입력을 받는지 체크하는 항목인데 마우스나 실시간 체크에서 문제점이 발견됬다 개선 사항이 필요하다.
        InputManager.Instance.KeyAction -= OnKeyboard;
        InputManager.Instance.KeyAction += OnKeyboard;
        //Debug.Log("Main Camera : " + mainCamera.name);
    }

    private void Update()
    {
        if (GameManager.Instance.isGameStop == true) { return; }

        if (isRuning == false && dodgeDelay > 2 && RunDelay > 1 && GameManager.Instance.playerStamina <= GameManager.Instance.playerMaxStamina)
        {
            GameManager.Instance.playerStamina += 1;
        }

        speedMultiplier = 1.0f;

        RunDelay += TimeManager.Instance.gameDeltaTime;
        dodgeDelay += TimeManager.Instance.gameDeltaTime;
        groundDelay += TimeManager.Instance.gameDeltaTime;
        Debug.Log("Run Delay :" + RunDelay);
        Debug.Log($"isMoving{isMoving}");
        #region KeyUp Bool
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            PlayerAnimator.SetBool("IsRun", false);
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            isMoving = false;
            PlayerAnimator.SetBool("IsMove", false);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            isMoving = false;
            PlayerAnimator.SetBool("IsMove", false);
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            isMoving = false;
            PlayerAnimator.SetBool("IsMove", false);
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            isMoving = false;
            PlayerAnimator.SetBool("IsMove", false);
        }
        #endregion

        currentState.Execute();
        Debug.Log(currentState.ToString());

        SetAniMatorBool?.Invoke("canSecondAttack", canSecondAttack);
        SetAniMatorBool?.Invoke("doingSecondAttack", doingSecondAttack);


        #region 인풋 매니저에서 입력이 잘 안되는부분을 모아서 업데이트로 옮겨봤다. 
        //그라운드 체크의 문제를 OncollisionStay를 이용해서 해결


        if (Input.GetKey(KeyCode.LeftShift) && GameManager.Instance.playerStamina > 0)
        {
            //뛰고 있으면 스테미나 감소
            if (GameManager.Instance.playerStamina > 0 && isMoving == true && RunDelay > 0.03)
            {
                RunDelay = 0;
                GameManager.Instance.playerStamina -= 1;
                isRuning = true;
            }
            PlayerAnimator.SetBool("IsRun", true);
            speedMultiplier = 2.0f; // Shift 키를 눌렀을 때 속도 배율 증가

            if (GameManager.Instance.playerStamina <= 0)
            {
                isRuning = false;
                PlayerAnimator.SetBool("IsRun", false);
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isRuning = false;
        }

        // 점프하는 모션 착지와 콜라이더 체크에 문제가 있다.
        if (Input.GetKeyDown(KeyCode.Space) && OnGround == true)
        {
            Debug.Log("점프!");
            rb.velocity = new Vector3(rb.velocity.x, 5.5f, rb.velocity.z);
            SetState(new PlayerJump(this));
        }
        // 공격을 담당하는 if문 시간이 없어서 연속공격을 구현하기 위해 이런 구조를 사용했지만 더 나은 구조를 생각해야한다.
        if (Input.GetKeyDown(KeyCode.Mouse0) && doingAttack == false)
        {
            StopCoroutine("DelayAttack");
            StartCoroutine("DelayAttack");
            StartCoroutine("CanSecondAttack");
            SetState(new PlayerAttack(this));
        }
        else if (Input.GetKeyDown(KeyCode.Mouse0) && doingSecondAttack == false)
        {
            if (canSecondAttack == true)
            {
                StopCoroutine("DelayAttack");
                StopCoroutine("DelaySecondAttack");
                StartCoroutine("DelaySecondAttack");
                SetState(new PlayerAttack(this));
            }
        }
        #endregion
    }
    //! Collision 체크시 제대로 체크하지 못하는 문제가 발생해서 Trigger체크 방식으로 변경
    private void OnCollisionEnter(Collision other)
    {

    }
    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            OnGround = true;
            groundDelay = 0f;
            SetAniMatorBool?.Invoke("OnGround", true);
            //StopAllCoroutines();
            if (groundCheckCoroutine != null)
            {
                StopCoroutine(groundCheckCoroutine);
                groundCheckCoroutine = null;
            }
        }
        if (other.gameObject.CompareTag("MonsterAttack") && onGetHit == false)
        {
            PlayerAnimator.SetBool("GetHit", true);
            //피격당하는 코드
            StartCoroutine(DelayOnGit());
        }
    }
    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            OnGround = false;
            if (groundCheckCoroutine != null)
            {
                StopCoroutine(groundCheckCoroutine);
            }
            groundCheckCoroutine = StartCoroutine(DelayAnimatorCheck(groundCheckDelay, OnGround, "OnGround"));
        }
    }

    private void OnKeyboard()
    {
        if (GameManager.Instance.isGameStop == true) { return; }

        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.F) && OnGround == true && GameManager.Instance.playerStamina > 10 && dodgeDelay > 2)
        {
            dodgeDelay = 0;
            GameManager.Instance.playerStamina -= 10;
            if (GameManager.Instance.playerStamina < 0)
            {
                GameManager.Instance.playerStamina = 0;
            }

            Debug.Log("구르기!");
            SetState(new PlayerDodge(this));
            //캐릭터의 정면을 구함
            Vector3 forward = transform.forward;
            //방향벡터에 속도를 곱한다.
            Vector3 dodge = forward * GameManager.Instance.speed * 2;
            //벡터를 캐릭터의 리지드바디에 적용한다.
            rb.velocity = dodge;
        }

        #region GetKey를 받는 WSAD Shift
        if (Input.GetKey(KeyCode.W))
        {
            isMoving = true;
            PlayerAnimator.SetBool("IsMove", true);
            SetState(new PlayerMove(this));
            moveDirection += mainCamera.transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            isMoving = true;
            PlayerAnimator.SetBool("IsMove", true);
            SetState(new PlayerMove(this));
            moveDirection -= mainCamera.transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            isMoving = true;
            PlayerAnimator.SetBool("IsMove", true);
            SetState(new PlayerMove(this));
            moveDirection -= mainCamera.transform.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            isMoving = true;
            PlayerAnimator.SetBool("IsMove", true);
            SetState(new PlayerMove(this));
            moveDirection += mainCamera.transform.right;
        }
        #endregion

        // 이동 방향의 Y축 값을 0으로 설정하여 수평 이동만 하도록 합니다.
        moveDirection.y = 0;


        if (moveDirection != Vector3.zero)
        {
            // 플레이어 회전
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f);

            // 플레이어 이동
            transform.position += TimeManager.Instance.playerTime * GameManager.Instance.speed * speedMultiplier * moveDirection.normalized;
        }
    }
    public void SetState(IStateBase newState)
    {
        if (currentState != null)
        { 
            currentState.Exit();
        }
        currentState = newState;
        currentState.Enter();
    }
    //! 애니메이터의 Bool설정에 딜레이를 주기위한 함수
    // 칼 맞음 온힛 트루 -> 라면 안맞음 -> 0.5초후 온힛 폴스 -> 
    private IEnumerator DelayAnimatorCheck(float DelayTime,bool controllerBool,string animatorBool)
    {
        yield return new WaitForSeconds(DelayTime);

        if (!controllerBool)
        {
            SetAniMatorBool?.Invoke(animatorBool,false);
        }
    }
    private IEnumerator DelayOnGit()
    {
        onGetHit = true;
        yield return new WaitForSeconds(0.5f);
        onGetHit = false;
    }
    private IEnumerator DelayAttack()
    {
        doingAttack= true;
        yield return new WaitForSeconds(1.49f);
        doingAttack= false;
    }
    private IEnumerator CanSecondAttack()
    {
        yield return new WaitForSeconds(0.85f);
        PlayerAnimator.SetBool("canSecondAttack", true);
        canSecondAttack = true;
        yield return new WaitForSeconds(0.59f);
        PlayerAnimator.SetBool("canSecondAttack", false);
        canSecondAttack = false;
    }
    private IEnumerator DelaySecondAttack()
    {
        PlayerAnimator.SetBool("doingSecondAttack", true);
        doingSecondAttack = true;
        doingAttack = true;
        yield return new WaitForSeconds(1.49f);
        PlayerAnimator.SetBool("doingSecondAttack", false);
        doingSecondAttack = false;
        doingAttack = false;
    }
}
