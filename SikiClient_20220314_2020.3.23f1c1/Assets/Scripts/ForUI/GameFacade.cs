using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 外观模式   中介者(持有各种系统)<para />
/// Siki 设计模式那个红警小demo里面的<para />
/// </summary> 
public class GameFacade
{
    #region 属性



    private static GameFacade _instance = new GameFacade();
    public static GameFacade Insance { get { return _instance; } }

    private GameFacade() { }




    public static RegisterUI registerUI;
    public static LoginUI loginUI;

    
    #endregion



    #region 生命
    public void Init()
    {
        registerUI = new RegisterUI();
        registerUI.Init();

        loginUI=new LoginUI();
        loginUI.Init();
    }
    public void Update()
    {
        registerUI.Update();

    }
    public void Release()
    {
        registerUI.Release();


    }
    /// <summary>
    /// 偷懒的写法
    /// </summary>
    /// <param name="registerTypeRT"></param>
    /// <param name="registerContent"></param>
    /// <param name="userName"></param>
    /// <param name="pwd"></param>
    public static void GetRegisterUIInfo(
        out RegisterType registerTypeRT,
        out string registerContent,
        out string userName,
        out string pwd)
    {
        registerTypeRT = registerUI.registerTypeRT;
        registerContent = registerUI.registerContent;
        userName = registerUI.userName;
        pwd = registerUI.pwd;
            

    }

    public static void GetLoginUIInfo(
        out LoginType loginTypeRT,
        out string loginContent,
        out string userName,
        out string pwd)
    {
        loginTypeRT = loginUI.loginTypeLT;
        loginContent = loginUI.loginContent;
        userName = loginUI.userName;
        pwd = loginUI.pwd;
                


    }
    #endregion

}

