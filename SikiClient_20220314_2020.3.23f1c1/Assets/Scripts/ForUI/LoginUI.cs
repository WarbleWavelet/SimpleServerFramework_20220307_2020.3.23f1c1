using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



    public class LoginUI : IBaseUI
    {
        #region 字段

    //
        private Button loginBtn;
    //
        private TextMeshProUGUI loginTypeText;
        private TextMeshProUGUI loginContentText;
        private TextMeshProUGUI userNameText;
        private TextMeshProUGUI pwdText;
    //
     public    LoginType loginTypeLT;
     public   string loginType ;
     public   string loginContent;
     public   string userName;
     public   string pwd ;

    public Button LoginBtn { get => loginBtn; set => loginBtn = value; }

    #endregion


    public override void Init()
        {
            base.Init();
            GameObject canvas = GameObject.Find("Canvas");
            mRootUI = UnityTool.FindChild(canvas, "登录");



            LoginBtn = UITool.FindChild<Button>(mRootUI, "按钮");
            //
            loginTypeText = UITool.FindChild<TextMeshProUGUI>(mRootUI, "方式_Dropdown_Label");
            loginContentText = UITool.FindChild<TextMeshProUGUI>(mRootUI, "方式内容_Text (TMP)");
            userNameText = UITool.FindChild<TextMeshProUGUI>(mRootUI, "用户名_Text (TMP)");
            pwdText = UITool.FindChild<TextMeshProUGUI>(mRootUI, "密码_Text (TMP)");

        LoginBtn.onClick.AddListener(OnLoginBtnClick);

        //默认
            loginContent = "13659260524";
            userName = "Ocean";
            pwd = "123456";


        Show();


        }

    void OnLoginBtnClick()
    {
        string loginType = loginTypeText.text;
        loginTypeLT = LoginType.Mail;
        if (loginType == "Mail")
            loginTypeLT = LoginType.Mail;
        else
            loginTypeLT = LoginType.Phone;



        string loginContent = loginContentText.text;
        string userName = userNameText.text;
        string pwd = pwdText.text;
    }
    
}



