using Cinemachine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeLookController : MonoBehaviour
{
    [FoldoutGroup("freeLook Camera")]
    public CinemachineFreeLook freeLookCamera;
    [FoldoutGroup("freeLook Camera")]
    public Transform playerViewPoint = default;

    [FoldoutGroup("Rotation Settings")]
    [SerializeField] private float rotationSpeed = default;

    [FoldoutGroup("Zoom Settings")]
    [SerializeField] private float zoomSpeed = default;
    [FoldoutGroup("Zoom Settings")]
    [SerializeField] private float minZoomDistance = default;
    [FoldoutGroup("Zoom Settings")]
    [SerializeField] private float maxZoomDistance = default;

    [FoldoutGroup("Height Settings")]
    [SerializeField]private float handleHeightSpeed = default;
    [FoldoutGroup("Height Settings")]
    [SerializeField] private float minHeight = default;
    [FoldoutGroup("Height Settings")]
    [SerializeField] private float maxHeight = default;

    //! 메인 카메라의 정보를 캐싱하기 위해서 Awake에서 게임매니저에 카메라 컨트롤러를 캐싱
    private void Awake()
    {

    }

    void Start()
    {
        freeLookCamera = GameManager.Instance.freeLookCamera;
        playerViewPoint = GameManager.Instance.playerController.transform.GetChild(0);

        freeLookCamera.LookAt = playerViewPoint;
        freeLookCamera.Follow = playerViewPoint;
        
        //변수를 초기화
        rotationSpeed = 50.0f;
        zoomSpeed = 10.0f;
        handleHeightSpeed = 100.0f;
        minZoomDistance = 5.0f;
        maxZoomDistance = 100.0f;
        minHeight = 0.3f;
        maxHeight = 2.0f;

        Debug.Log("Player View Point: " + playerViewPoint.name);
    }

    void Update()
    {
        HandleCameraRotation();
        HandleCameraZoom();
        HandleCameraHeight();
    }

    private void HandleCameraRotation()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime;

            // CinemachineFreeLook의 회전을 처리합니다.
            float newXValue = freeLookCamera.m_XAxis.Value + mouseX * rotationSpeed * 100f;
            float newYValue = freeLookCamera.m_YAxis.Value - mouseY * rotationSpeed;
            freeLookCamera.m_XAxis.Value = Mathf.Lerp(freeLookCamera.m_XAxis.Value, newXValue, Time.deltaTime);
            freeLookCamera.m_YAxis.Value = Mathf.Lerp(freeLookCamera.m_YAxis.Value, newYValue, Time.deltaTime);
        }
    }

    private void HandleCameraZoom()
    {
        float cameraScroll = Input.GetAxis("Mouse ScrollWheel");

        if (cameraScroll != 0)
        {
            float newDistance = freeLookCamera.m_Lens.FieldOfView - cameraScroll * zoomSpeed;
            freeLookCamera.m_Lens.FieldOfView = Mathf.Clamp(newDistance, minZoomDistance, maxZoomDistance);
        }
    }

    private void HandleCameraHeight()
    {
        if (Input.GetMouseButton(2)) // 스크롤 버튼을 누른 상태
        {
            float mouseY = Input.GetAxis("Mouse Y") * handleHeightSpeed * Time.deltaTime;

            // 플레이어 자식 오브젝트의 로컬 좌표를 사용하여 높이를 변경합니다.
            float newLocalHeight = playerViewPoint.localPosition.y + mouseY;

            // 카메라의 높이를 제한하려면, 이 코드를 사용
            newLocalHeight = Mathf.Clamp(newLocalHeight, minHeight, maxHeight);

            // 플레이어 자식 오브젝트의 로컬 높이를 새로운 높이로 변경하고, 카메라의 LookAt 위치를 업데이트합니다.
            Vector3 newPlayerChildLocalPosition = new Vector3(playerViewPoint.localPosition.x, newLocalHeight, playerViewPoint.localPosition.z);
            playerViewPoint.localPosition = Vector3.Lerp(playerViewPoint.localPosition, newPlayerChildLocalPosition, Time.deltaTime);
            freeLookCamera.LookAt.position = playerViewPoint.position;
        }
    }
}
