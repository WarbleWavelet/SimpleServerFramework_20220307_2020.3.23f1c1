using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameStart : MonoBehaviour
{

    [SerializeField] string ip_Address = "127.0.0.1";
    [SerializeField] int ip_Port = 8011;


    #region 生命
    void Start()
    {
        GameFacade.Insance.Init();
        NetManager.Instance.Connect(ip_Address, ip_Port);
        StartCoroutine(NetManager.Instance.CheckNet());

        GameFacade.registerUI.RegisterBtn.onClick.AddListener(Register);//这样写偷懒了
        GameFacade.loginUI.LoginBtn.onClick.AddListener(Login);//这样写偷懒了


    }

    // Update is called once per frame
    void Update()
    {
        GameFacade.Insance.Update();
        NetManager.Instance.Update();
        TestFenBao();
    }

    private void OnApplicationQuit()
    {
        NetManager.Instance.Close();
    }
    #endregion

    #region 辅助2
    /// <summary>测试分包</summary>
    void TestFenBao()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            for (int i = 0; i < 7; i++)
            {
                ProtocolMgr.SocketTest();
            }
        }
    }

    
    /// <summary>
    /// 注册<para />
    /// 实际UI要先判断数据正确，才提交到服务器<para />
    /// </summary>
    void Register()
    {
       GameFacade.GetRegisterUIInfo(
                out RegisterType registerTypeRT,
                out string registerContent,
                out string userName,
                out string pwd);

        ProtocolMgr.Register(registerTypeRT,registerContent,userName,pwd, (res) =>
        {
            if (res == RegisterResult.AlreadyExist)
            {
                Debug.LogError("该手机号已经注册过了");
            }
            else if (res == RegisterResult.WrongCode)
            {
                Debug.LogError("验证码错误");
            }
            else if (res == RegisterResult.Forbidden)
            {
                Debug.LogError("改账户禁止铸错，联系客服！");
            }
            else if (res == RegisterResult.Success)
            {
                Debug.Log("注册成功");
            }
        });
    }
    /// <summary>
    /// 登录
    /// </summary>
    void Login()
    {

        GameFacade.GetLoginUIInfo(
            out LoginType loginTypeRT,
            out string loginContent,
            out string userName,
            out string pwd
        );
        ProtocolMgr.Login(loginTypeRT, loginContent, userName, (res, restoken) =>
        {
            if (res == LoginResult.Success)
            {
                Debug.Log("登录成功");
            }
            else if (res == LoginResult.Failed)
            {
                Debug.LogError("登录失败");
            }
            else if (res == LoginResult.WrongPwd)
            {
                Debug.LogError("密码错误");
            }
            else if (res == LoginResult.UserNotExist)
            {
                Debug.LogError("用户不存在");
            }
        });
    }
    #endregion

}
