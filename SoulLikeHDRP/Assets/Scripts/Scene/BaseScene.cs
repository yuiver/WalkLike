using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseScene : MonoBehaviour
{
    public SceneType SceneType = SceneType.Unknown;
    protected bool _init = false;

    public void Awake()
    {
        Init();
    }

    protected virtual bool Init()
    {
        if (_init == true)
            return false;

        _init = true;
        ResourceManager.Instance.Create();
        GameManager.Instance.Create();
        SoundManager.Instance.Create();
        TimeManager.Instance.Create();
        InputManager.Instance.Create();

        GameObject go = GameObject.Find("EventSystem");
        if (go == null)
        {
            ResourceManager.Instance.Instantiate("EventSystem", null, (go) =>
            {
                go.name = "@EventSystem";
            });
        }

        return true;
    }

    public virtual void Clear()
    {
        GameManager.Instance.Clear();
    }
}
