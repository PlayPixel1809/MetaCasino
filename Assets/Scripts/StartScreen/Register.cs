using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Register : MonoBehaviour
{
    public static Register ins;
    void Awake(){ ins = this; }

    public bool validateInputs;
    public bool verifyEmail;
    
    public float startBalance = 100000;

    public InputField signupUsername;
    public InputField signupEmail;
    public InputField signupPassword;
    public InputField signupPasswordConfirm;
    public Toggle termsAndConditions;


    private List<string> securityCodes;

    private string availableEmail = string.Empty;
    private string verifiedEmail = string.Empty;

    void OnEnable()
    {
        ResetFields();
    }

    public void BackBtn()
    {
        gameObject.SetActive(false);
        Login.ins.gameObject.SetActive(true);
    }

    public void SubmitBtn(Transform btn)
    {
        Debug.Log("Register.SubmitBtn");
        
        if (string.IsNullOrEmpty(signupUsername.text) )                             { NoticeUtils.ins.ShowOneBtnAlert("Username cannot be empty"); return; }
        if (signupUsername.text.IndexOf(" ") > -1 )                                 { NoticeUtils.ins.ShowOneBtnAlert("Username cannot have empty spaces"); return; }
        if (signupUsername.text.Length > 15)                                        { NoticeUtils.ins.ShowOneBtnAlert("Username cannot be more than 15 characters long"); return; }
        if (string.IsNullOrEmpty(signupEmail.text))                                 { NoticeUtils.ins.ShowOneBtnAlert("Email cannot be empty");    return; }
        if (signupEmail.text.IndexOf(" ") > -1)                                     { NoticeUtils.ins.ShowOneBtnAlert("Email cannot have empty spaces"); return; }
        if (signupEmail.text.IndexOf("@") < 0 || signupEmail.text.IndexOf(".") < 0) { NoticeUtils.ins.ShowOneBtnAlert("Invalid Email");            return; }
        if (string.IsNullOrEmpty(signupPassword.text))                              { NoticeUtils.ins.ShowOneBtnAlert("Password cannot be empty"); return; }
        if (signupPassword.text.IndexOf(" ") > -1)                                  { NoticeUtils.ins.ShowOneBtnAlert("Password cannot have empty spaces"); return; }
        if (signupPassword.text.Length < 6)                                         { NoticeUtils.ins.ShowOneBtnAlert("Password length cannot be less then 6 letters"); return; }
        if (signupPassword.text != signupPasswordConfirm.text)                      { NoticeUtils.ins.ShowOneBtnAlert("Passwords don't match");    return; }
        if (!termsAndConditions.isOn)                                               { NoticeUtils.ins.ShowOneBtnAlert("Please agree to terms and conditions"); return; }

        TryRegisteringUser();
    }

    void TryRegisteringUser()
    {
        if (signupEmail.text != availableEmail) { CheckEmailAvailibility(); return; }
        if (verifyEmail && signupEmail.text != verifiedEmail)  { VerifyEmail(); return; }

        RegisterIfUsernameAvailable((playFabId) =>
        {
            SaveAdditionalDataToPlayFab(() =>
            {
                PlayerPrefs.SetString("email", signupEmail.text);
                PlayerPrefs.SetString("password", signupPassword.text);
                CreateLocalUser(playFabId);

                NoticeUtils.ins.ShowLoadingAlert("Loading Lobby");
                Utils.InvokeDelayedAction(0, () => { UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby"); });
            });
        });
    }


    void RegisterIfUsernameAvailable(Action<string> onRegister = null)
    {
        NoticeUtils.ins.ShowLoadingAlert("Registering User");

        RegisterPlayFabUserRequest req = new RegisterPlayFabUserRequest
        {
            Email = signupEmail.text,
            DisplayName = signupUsername.text,
            Password = signupPassword.text,
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(req,
        res =>
        {
            Debug.Log(res.PlayFabId);
            NoticeUtils.ins.HideLoadingAlert();
            onRegister?.Invoke(res.PlayFabId);
        },
        err =>
        {
            //NameNotAvailable = 1058
            Debug.Log("Error: " + (err.ErrorMessage));
            NoticeUtils.ins.HideLoadingAlert();
            NoticeUtils.ins.ShowOneBtnAlert("Error: " + (err.ErrorMessage));
        });
    }

    void SaveAdditionalDataToPlayFab(Action onDataSave)
    {
        NoticeUtils.ins.ShowLoadingAlert("Saving Additional Data");

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>()
            {
                {"balance", startBalance.ToString()}
            }
        },
        result =>
        {
            onDataSave?.Invoke();
            Debug.Log("Successfully updated user data");
        },
        error =>
        {
            Debug.Log("Error occurred saving additional data");
            Debug.Log(error.GenerateErrorReport());
            NoticeUtils.ins.HideLoadingAlert();
            NoticeUtils.ins.ShowOneBtnAlert("Error occurred saving additional data.");
        });
    }


    void CreateLocalUser(string playfabId)
    {
        User user = new GameObject("User").AddComponent<User>();
        user.playfabId = playfabId;
        user.username = signupUsername.text;
        User.CreateLocalUser(user);
    }

    void CheckEmailAvailibility()
    {
        NoticeUtils.ins.ShowLoadingAlert("Checking email availibilty");
        //login with signupEmail and wrong password 
        PlayFabClientAPI.LoginWithEmailAddress(new LoginWithEmailAddressRequest() { Email = signupEmail.text, Password = "rdgfnbdytjagagehthjthmuykkuyktnsd" }, null,
        err =>
        {
            Debug.Log("Error: " + (err.ErrorMessage));
            Debug.Log("Error: " + ((int)err.Error));

            NoticeUtils.ins.HideLoadingAlert();
            if ((int)err.Error == 1001) //AccountNotFound = 1001
            {
                availableEmail = signupEmail.text;
                TryRegisteringUser();
            }
            if ((int)err.Error == 1142) //InvalidEmailOrPassword = 1142
            { NoticeUtils.ins.ShowOneBtnAlert("Email already taken"); }
        });
    }

   

    public void ResetFields()
    {
        securityCodes = new List<string>();
        availableEmail = string.Empty;
        verifiedEmail = string.Empty;

        signupUsername.text = string.Empty;
        signupPassword.text = string.Empty;
        signupPasswordConfirm.text = string.Empty;
        signupEmail.text = string.Empty;
        termsAndConditions.isOn = false;
    }

    public void VerifyEmail()
    {
        int securityCode = UnityEngine.Random.Range(100000, 999999);
        securityCodes.Add(securityCode.ToString());
        Mail.SendEmail(signupEmail.text, "Meta Casino Registration Security Code", securityCode.ToString());

        NoticeUtils.ins.ShowInputAlert("Please enter the 6-digit security code sent to " + signupEmail.text, (i, val, inputAlert) =>
        {
            if (i == 0)
            {
                if (string.IsNullOrEmpty(val)) { NoticeUtils.ins.ShowOneBtnAlert("Security Code cannot be empty"); return; }

                if (securityCodes.Contains(val))
                {
                    inputAlert.SetActive(false);
                    verifiedEmail = signupEmail.text;
                    TryRegisteringUser();
                }
                else { NoticeUtils.ins.ShowOneBtnAlert("Wrong Security Code"); }
            }

            if (i == 1) { inputAlert.SetActive(false); }

        }, "Security code...");
    }
}
