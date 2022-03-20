using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;


/// <summary>接口无法声明字段</summary>
public abstract class IBaseUI
{
    protected GameFacade mFacade;
    /// <summary>UI对象</summary>
    public GameObject mRootUI;
    #region 抽象不一定实现
    /// <summary>持有中介者</summary>
    public virtual void Init() {
        mFacade = GameFacade.Insance;
    }
    public virtual void Update() { }
    public virtual void Release() { }
    #endregion


    /// <summary>显示</summary>
    protected void Show()
    {
        mRootUI.SetActive(true);
    }
    /// <summary>隐藏</summary>
    protected void Hide()
    {
        mRootUI.SetActive(false);
    }
}
