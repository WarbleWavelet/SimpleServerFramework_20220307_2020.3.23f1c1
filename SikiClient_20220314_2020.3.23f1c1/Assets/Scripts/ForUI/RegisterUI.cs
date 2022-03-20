using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



    public class RegisterUI : IBaseUI
    {
        #region 字段

    //
        private Button registerBtn;
    //
        private TextMeshProUGUI registerTypeText;
        private TMP_InputField registerContentText;
        private TMP_InputField userNameText;
        private TMP_InputField pwdText;
    //
     public    RegisterType registerTypeRT;
     public   string registerType ;
     public   string registerContent;
     public   string userName;
     public   string pwd ;

    public Button RegisterBtn { get => registerBtn; set => registerBtn = value; }

    #endregion


    public override void Init()
        {
            base.Init();
            GameObject canvas = GameObject.Find("Canvas");
            mRootUI = UnityTool.FindChild(canvas, "注册");



            RegisterBtn = UITool.FindChild<Button>(mRootUI, "按钮");
            //
            registerTypeText = UITool.FindChild<TextMeshProUGUI>(mRootUI, "方式_Dropdown_Label");
            registerContentText = UITool.FindChild<TMP_InputField>(mRootUI, "方式内容_InputField (TMP)");
            userNameText = UITool.FindChild<TMP_InputField>(mRootUI, "用户名_InputField (TMP)");
            pwdText = UITool.FindChild<TMP_InputField>(mRootUI, "密码_InputField (TMP)");

            RegisterBtn.onClick.AddListener(OnRegisterBtnClick);

            //默认
            registerContentText.text = "13659260524";
            userNameText.text = "Ocean";
            pwdText.text = "123456";


        Show();


        }

   public void OnRegisterBtnClick()
    {
        string registerType = registerTypeText.text;
        registerTypeRT = RegisterType.Mail;
        if (registerType == "Mail")
            registerTypeRT = RegisterType.Mail;
        else
            registerTypeRT = RegisterType.Phone;

        

        registerContent = registerContentText.text;
        userName = userNameText.text;
        pwd = pwdText.text;
    }


    
}



