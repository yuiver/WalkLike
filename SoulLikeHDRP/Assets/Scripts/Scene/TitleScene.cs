using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine;
using TMPro;

public class TitleScene : BaseScene
{
    public TMP_Text VersionInfo = default;

    protected override bool Init()
    {
        if (base.Init() == false)
            return false;

        SceneType = SceneType.TitleScene;

        StartCoroutine(CoWaitLoad());

        string appVersion = Application.version;
        VersionInfo.text = $" Version:{appVersion}";


        return true;
    }
    IEnumerator CoWaitLoad()
    {
        while (GameManager.Instance.IsLoaded == false)
            yield return null;
    }
    public void ClickStart()
    {
        SceneManagerEX.Instance.ChangeScene(SceneType.GameScene);
    }
    public void CheckHardwareForRTX()
    { 
        //현재 그래픽 카드의 정보를 가져오기
        GraphicsDeviceType deviceType = SystemInfo.graphicsDeviceType;

        // RTX 지원 가능한지 체크
        bool RTXSupported = (deviceType == GraphicsDeviceType.Direct3D12) && (SystemInfo.supportsRayTracing);

        // 현재 랜더링파이프라인(HDRP) 세팅을 가져오기
        HDRenderPipelineAsset hdrpAsset = GraphicsSettings.currentRenderPipeline as HDRenderPipelineAsset;

        var hdrpGlobalSettings = RenderPipelineGlobalSettings.Instantiate(hdrpAsset);

        if (RTXSupported == true && hdrpAsset != null)
        {
            // 현재 랜더링 파이프라인 설정을 복사해서 캐싱한다.
            RenderPipelineSettings settings = hdrpAsset.currentPlatformRenderPipelineSettings;

            // RTX를 지원한다면 활성화하고 지원하지 않는다면 비활성화한다.
            settings.supportRayTracing = RTXSupported;

            // 캐싱한 파이프라인 설정을 현재 파이프라인에 적용한다. ( 그냥 접근한다면 구조체라서 접근할 수 없기 때문에 이런식으로 캐싱 후 변경하는 방식을 사용했다. )
            //hdrpAsset.currentPlatformRenderPipelineSettings = settings;

            // RTX 활성화/비활성화
        }
            

    }
}
