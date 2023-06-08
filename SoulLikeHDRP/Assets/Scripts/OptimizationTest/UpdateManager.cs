using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 업데이트를 한번에 관리해서 오버헤드를 줄여주는 매니저를 구현하기 위한 테스트 코드입니다.
/// </summary>
public class UpdateManager : GSingleton<UpdateManager>
{
    public List<IUpdatable> updatables = new List<IUpdatable>();

    protected override void Update()
    {
        foreach (var updatable in updatables)
        {
            //if (updatable is MonoBehaviour monoBehaviour && monoBehaviour.gameObject.activeInHierarchy) 을 이용한다면 비활성화 체크가 가능한데 효율을 위해서 작성한 코드에서 이런걸 사용한다면 나쁘다고 생각되서 고민중
            //만약 객체를 Destroy하면서 호출이 동시에 발생한다면의 예외처리
            if (updatable != null)
            {
                updatable.DoUpdate();
            }
        }
    }
}

//! 이를 위한 인터페이스 구현
public delegate void UpdateDelegate();
public interface IUpdatable
{
    UpdateDelegate DoUpdate { get; }
}


//! 델리게이트를 사용하는 예시 함수
public class UpdatableComponent : MonoBehaviour, IUpdatable
{
    private bool onDate;

    private void Awake()
    {
        UpdateManager.Instance.updatables.Add(this);
        onDate = true;
    }

    private void OnEnable()
    {
        if (onDate == false)
        {
            UpdateManager.Instance.updatables.Add(this);
            onDate = true;
        }
    }

    public UpdateDelegate DoUpdate
    {
        get
        {
            return () =>
            {
                // 여기에 객체를 업데이트하는 로직을 구현
            };
        }
    }

    //! Destroy된 객체를 리스트에서 제거
    private void OnDestroy()
    {
        UpdateManager.Instance.updatables.Remove(this);
        onDate = false;
    }
}
