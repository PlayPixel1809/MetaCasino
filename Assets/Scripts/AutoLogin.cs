using PlayFab;
using PlayFab.ClientModels;
using ReadyPlayerMe.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AutoLogin : MonoBehaviour
{
    

    [Space(30)]
    public ResponseInfo getTokenResponse;
    public ResponseInfo validateTokenResponse;

    //localhost:54380/?email=digitalmetahuman@gmail.com&password=testpass123
    void Start()
    {
        NoticeUtils.ins.ShowOneBtnAlert("App Loaded");

        //JsMethods.GetToken();


        /*
        string email = "digitalmetahuman@gmail.com";
        string password = "testpass123";



        if (Application.platform == RuntimePlatform.WebGLPlayer) 
        {
            email = JsMethods.GetEmailFromUrl();
            password = JsMethods.GetPasswordFromUrl();
        }

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(email))
        {
            NoticeUtils.ins.ShowOneBtnAlert("email or password not set in the url, Set email and password in the url and reload the page");
        }
        else 
        {
            StartCoroutine(GetToken(email, password, (text) =>
            {
                Debug.Log(text);
                getTokenResponse = JsonUtility.FromJson<ResponseInfo>(text);
                StartCoroutine(ValidateToken(getTokenResponse.data.token, (text) =>
                {
                    Debug.Log(text);
                    validateTokenResponse = JsonUtility.FromJson<ResponseInfo>(text);
                    LoginWithEmail(getTokenResponse.data.email);
                }));
            }));
        }*/

    }

    public void ShowTokenError(string error)
    {
        NoticeUtils.ins.ShowOneBtnAlert("Error: " + error);
    }


    public void SetToken(string token)
    {
        StartCoroutine(ValidateToken(token, (text) =>
        {
            Debug.Log(text);
            validateTokenResponse = JsonUtility.FromJson<ResponseInfo>(text);
            LoginWithEmail(getTokenResponse.data.email);
        }));
    }





    IEnumerator GetToken(string username, string password, Action<string> onSuccess)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        UnityWebRequest www = UnityWebRequest.Post("https://theomniverse.city/wp-json/jwt-auth/v1/token", form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        www.SetRequestHeader("Cookie", "PHPSESSID=8e86e28cc0999fecc90aa7dcac0cb3e4");

        NoticeUtils.ins.ShowLoadingAlert("Geting Token, email: " + username);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ConnectionError) { NoticeUtils.ins.ShowOneBtnAlert(www.error); } else { onSuccess?.Invoke(www.downloadHandler.text); }
    }


    IEnumerator ValidateToken(string token, Action<string> onSuccess)
    {
        WWWForm form = new WWWForm();
        UnityWebRequest www = UnityWebRequest.Post("https://theomniverse.city/wp-json/jwt-auth/v1/token/validate", form);
        www.SetRequestHeader("Authorization", "Bearer " + token);
        NoticeUtils.ins.ShowLoadingAlert("Validating Token");

        yield return www.SendWebRequest();
        NoticeUtils.ins.HideLoadingAlert();
        if (www.result == UnityWebRequest.Result.ConnectionError) { NoticeUtils.ins.ShowOneBtnAlert(www.error); } else { onSuccess?.Invoke(www.downloadHandler.text); }
    }


    void LoginWithEmail(string email)
    {
        NoticeUtils.ins.ShowLoadingAlert("Login");
        User.onCreateLocalUser = delegate 
        {
            User.onCreateLocalUser = null;
            User.localUser.readyPlayerMeAvatarUrl = validateTokenResponse.data.user.avatar;
            //User.localUser.username = getTokenResponse.data.firstName;

            LoadPlayerMeAvatar(validateTokenResponse.data.user.avatar);
        };

        User.LoginAndCreateLocalUser(email, "12345678", (loginResult)=>{ loginResult.InfoResultPayload.AccountInfo.TitleInfo.DisplayName = getTokenResponse.data.firstName; }, 
        (error)=> 
        {
            RegisterByEmail(email, (playFabId) => { LoginWithEmail(email); });
        });
    }


    void RegisterByEmail(string email, Action<string> onRegister = null)
    {
        NoticeUtils.ins.ShowLoadingAlert("Registering User");
        RegisterPlayFabUserRequest req = new RegisterPlayFabUserRequest
        {
            Email = email,
            //DisplayName = signupUsername.text,
            Password = "12345678",
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



    void LoadPlayerMeAvatar(string avatarUrl)
    {
        NoticeUtils.ins.ShowLoadingAlert("Loading Ready Player Me Avatar");
        var avatarLoader = new AvatarObjectLoader();
        // use the OnCompleted event to set the avatar and setup animator
        avatarLoader.OnCompleted += (_, args) =>
        {
            User.localUser.readyPlayerMeAvatar = args.Avatar;
            AvatarAnimatorHelper.SetupAnimator(args.Metadata.BodyType, User.localUser.readyPlayerMeAvatar);
            User.localUser.readyPlayerMeAvatar.transform.parent = User.localUser.transform;
            User.localUser.readyPlayerMeAvatar.SetActive(false);

            NoticeUtils.ins.ShowLoadingAlert("Loading Casino");
            Utils.InvokeDelayedAction(0, () => { UnityEngine.SceneManagement.SceneManager.LoadScene("Casino"); });
        };
        avatarLoader.LoadAvatar(avatarUrl);
    }


































    [System.Serializable]
    public class ResponseInfo
    {
        public DataInfo data;
    }

    [System.Serializable]
    public class DataInfo
    {
        public string token;
        public string email;
        public string firstName;
        public UserInfo user;

    }

    [System.Serializable]
    public class UserInfo
    {
        public string avatar;
    }
}
