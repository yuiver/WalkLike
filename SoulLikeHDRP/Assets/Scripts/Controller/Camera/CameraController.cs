using Sirenix.OdinInspector;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [FoldoutGroup("Virtual Camera")]
    public CinemachineVirtualCamera virtualCamera;

    [FoldoutGroup("Rotation Settings")]
    [SerializeField] private float rotationSpeed = default;

    [FoldoutGroup("Zoom Settings")]
    [SerializeField] private float zoomSpeed = default;
    [FoldoutGroup("Zoom Settings")]
    [SerializeField] private float minZoomDistance = default;
    [FoldoutGroup("Zoom Settings")]
    [SerializeField] private float maxZoomDistance = default;

    [FoldoutGroup("Height Settings")]
    [SerializeField] private float minHeight = default;
    [FoldoutGroup("Height Settings")]
    [SerializeField] private float maxHeight = default;

    private CinemachineFramingTransposer framingTransposer;
    private CinemachinePOV cinemachinePOV;

    void Start()
    {
        //변수를 초기화
        rotationSpeed = 50.0f;
        zoomSpeed = 10.0f;
        minZoomDistance = 1.0f;
        maxZoomDistance = 5.0f;
        minHeight = 1.0f;
        maxHeight = 10.0f;

        framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        cinemachinePOV = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
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

            // CinemachinePOV의 회전을 처리합니다.
            cinemachinePOV.m_HorizontalAxis.Value += mouseX * rotationSpeed;
            cinemachinePOV.m_VerticalAxis.Value -= mouseY * rotationSpeed;

            // CinemachinePOV의 Y축 회전 값을 제한합니다.
            float clampedYAxis = cinemachinePOV.m_VerticalAxis.Value % 360f;
            cinemachinePOV.m_VerticalAxis.Value = clampedYAxis;
        }
    }

    private void HandleCameraZoom()
    {
        float cameraScroll = Input.GetAxis("Mouse ScrollWheel");

        if (cameraScroll != 0)
        {
            float newDistance = framingTransposer.m_CameraDistance - cameraScroll * zoomSpeed;
            framingTransposer.m_CameraDistance = Mathf.Clamp(newDistance, minZoomDistance, maxZoomDistance);
        }
    }

    private void HandleCameraHeight()
    {
        if (Input.GetMouseButton(2)) // 스크롤 버튼을 누른 상태
        {
            float mouseY = Input.GetAxis("Mouse Y") * zoomSpeed * Time.deltaTime;
            float newHeight = virtualCamera.transform.position.y + mouseY;

            // 카메라의 높이를 제한하려면, 이 코드를 사용하세요.
            newHeight = Mathf.Clamp(newHeight, minHeight, maxHeight);

            virtualCamera.transform.position = new Vector3(virtualCamera.transform.position.x, newHeight, virtualCamera.transform.position.z);
        }
    }
}
