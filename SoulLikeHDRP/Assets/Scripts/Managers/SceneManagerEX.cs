using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEX : GSingleton<SceneManagerEX>
{
    private SceneType _curSceneType = SceneType.Unknown;

    public SceneType CurrentSceneType
    {
        get
        {
            if (_curSceneType != SceneType.Unknown)
                return _curSceneType;
            return CurrentScene.SceneType;
        }
        set { _curSceneType = value; }
    }
    public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }

    public void ChangeScene(SceneType type)
    {
        GFunc.Log(CurrentScene);
        CurrentScene.Clear();

        _curSceneType = type;
        SceneManager.LoadScene(GetSceneName(type));
    }

    string GetSceneName(SceneType type)
    {
        string name = System.Enum.GetName(typeof(SceneType), type);
        char[] letters = name.ToLower().ToCharArray();
        letters[0] = char.ToUpper(letters[0]);
        return new string(letters);
    }
}
