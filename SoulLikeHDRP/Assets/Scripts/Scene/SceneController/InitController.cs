using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneManagerEX.Instance.ChangeScene(SceneType.TitleScene);
    }
}
