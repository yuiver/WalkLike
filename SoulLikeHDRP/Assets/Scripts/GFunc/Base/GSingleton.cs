using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GSingleton<T> : GComponent where T : GSingleton<T>
{
    private static T _instance = default;   // 유일성이 보장됨
    public static T Instance  
    {
        get
        { 
            if(_instance == default || _instance == null)
            {
                GameObject go = GameObject.Find("@Managers");
                if(go == null)
                {
                    go = new GameObject { name = "@Managers" };
                    DontDestroyOnLoad(go);
                }
                _instance = go.AddComponent<T>();
            }       // if: 인스턴스가 비어 있을 때 새로 인스턴스화 한다

            // 여기서 부터는 인스턴스가 절대 비어있을일은 없다.
            return _instance;
        }
    }

    public void Create()
    {
        this.Init();
    }       // Create()

    protected virtual void Init()
    {
        /* Do something */
    }

    public virtual void Clear()
    {
        //메모리 정리
        SoundManager.Instance.Clear();
    }

}
