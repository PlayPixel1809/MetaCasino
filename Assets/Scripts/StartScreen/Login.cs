using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Globalization;

public class Login : MonoBehaviour
{
    public static Login ins;
    void Awake() { ins = this; }

    public GameObject registerByEmailScreen;

    public InputField loginEmail;
    public InputField loginPassword;
    public Toggle rememberMe;
    public LoginResult loginResult;

    void Start()
    {
        if (PlayerPrefs.GetInt("rememberLogin") == 1) 
        {
            rememberMe.isOn = true;
            if (PlayerPrefs.HasKey("email"))    { loginEmail.text = PlayerPrefs.GetString("email"); }
            if (PlayerPrefs.HasKey("password")) { loginPassword.text = PlayerPrefs.GetString("password"); }
        }
        else 
        {rememberMe.isOn = false; }
        rememberMe.onValueChanged.AddListener(RememberLoginToggle);
    }

    public void SubmitBtn(Transform btn)
    {
        if (!ValidateEmail(loginEmail.text)) { return; }
        if (string.IsNullOrEmpty(loginPassword.text)) { NoticeUtils.ins.ShowOneBtnAlert("Password cannot be empty"); return; }

        User.onCreateLocalUser += () => 
        {
            PlayerPrefs.SetString("email", loginEmail.text);
            PlayerPrefs.SetString("password", loginPassword.text);
            NoticeUtils.ins.ShowLoadingAlert("Loading Lobby");
            Utils.InvokeDelayedAction(0, () => { UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby"); });
        };
        User.LoginAndCreateLocalUser(loginEmail.text, loginPassword.text);
    }
    
    void RememberLoginToggle(bool b)
    {
        Debug.Log("LoginRememberToggle");
        if (PlayerPrefs.GetInt("rememberLogin") == 0) { PlayerPrefs.SetInt("rememberLogin", 1); }
        else { PlayerPrefs.SetInt("rememberLogin", 0); }
    }

    public void RegisterByEmailBtn()
    {
        gameObject.SetActive(false);
        registerByEmailScreen.SetActive(true);
    }

    public void ResetPasswordBtn()
    {
        NoticeUtils.ins.ShowInputAlert("Please enter your registered email.", (i, val, inputAlert) =>
        {
            if (i == 0)
            {
                if (!ValidateEmail(val)) { return; }

                NoticeUtils.ins.ShowLoadingAlert("Please Wait");
                SendAccountRecoveryEmailRequest req = new SendAccountRecoveryEmailRequest()
                { 
                    Email = val,
                    TitleId = "B6B54"
                };

                PlayFabClientAPI.SendAccountRecoveryEmail(req,
                res =>
                {
                    NoticeUtils.ins.HideLoadingAlert();
                    inputAlert.SetActive(false);
                    NoticeUtils.ins.ShowOneBtnAlert("Password reset link has been sent to " + val);
                },
                err =>
                {
                    Debug.Log("Error: " + (err.ErrorMessage));
                    NoticeUtils.ins.HideLoadingAlert();
                    NoticeUtils.ins.ShowOneBtnAlert("Error: " + (err.ErrorMessage));
                });
            }

            if (i == 1) { inputAlert.SetActive(false); }

        }, "Email...");
    }

    
    bool ValidateEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) { NoticeUtils.ins.ShowOneBtnAlert("Email cannot be empty."); return false; }
        if (email.IndexOf(" ") > -1) { NoticeUtils.ins.ShowOneBtnAlert("Email cannot have empty spaces."); return false; }
        if (email.IndexOf("@") < 0 || email.IndexOf(".") < 0) { NoticeUtils.ins.ShowOneBtnAlert("Invalid Email"); return false; }
        return true;
    }
}
