using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInit : BaseScene
{
    protected override bool Init()
    {
        if (base.Init() == false)
            return false;

        SceneType = SceneType.TestInit;

        StartCoroutine(CoWaitLoad());

        return true;
    }
    IEnumerator CoWaitLoad()
    {
        while (GameManager.Instance.IsLoaded == false)
            yield return null;
    }
}
