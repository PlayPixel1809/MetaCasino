using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;


public partial class User : MonoBehaviour
{
    public static User ins;
    public void Awake() { ins = this; }

    public static User localUser;
    public static Action onCreateLocalUser;

    public string playfabId;
    public string username = "DemoUser";
    public float balance = 100000;
    public string readyPlayerMeAvatarUrl;
    public GameObject readyPlayerMeAvatar;

    [HideInInspector] public int roomIndex;
    public DateTime currentTime;
    public string currentTimeString;

    public LoginResult loginResult;

   
    
    public static void LoginAndCreateLocalUser(string playFabEmail, string password, Action<LoginResult> onSuccess = null, Action<PlayFabError> onFailed = null)
    {
        NoticeUtils.ins.ShowLoadingAlert("Login user");


        GetPlayerCombinedInfoRequestParams parameters = new GetPlayerCombinedInfoRequestParams() { GetUserAccountInfo = true, GetUserData = true };
        PlayFabClientAPI.LoginWithEmailAddress(new LoginWithEmailAddressRequest() { Email = playFabEmail, Password = password, InfoRequestParameters = parameters },
        res =>
        {
            NoticeUtils.ins.HideLoadingAlert();
            onSuccess?.Invoke(res);
            CreateUser(res);
        },
        err =>
        {
            Debug.Log("Error: " + (err.ErrorMessage));
            NoticeUtils.ins.HideLoadingAlert();
            //NoticeUtils.ins.ShowOneBtnAlert((err.ErrorMessage));
            onFailed?.Invoke(err);
        });
    }


    public static void CreateUser(LoginResult loginResult)
    {
        User user = new GameObject("LocalUser").AddComponent<User>();
        user.loginResult = loginResult;
        Dictionary<string, UserDataRecord> userData = loginResult.InfoResultPayload.UserData;

        user.playfabId = loginResult.PlayFabId;
        user.username = loginResult.InfoResultPayload.AccountInfo.TitleInfo.DisplayName;

        if (userData.ContainsKey("balance"))                    { user.balance = float.Parse(userData["balance"].Value); }

        CreateLocalUser(user);
    }

    public static void CreateLocalUser(User user)
    {
        localUser = user;
        PhotonNetwork.LocalPlayer.NickName = localUser.username;
        localUser.gameObject.AddComponent<SceneData>();
        localUser.GetServerTime();
        DontDestroyOnLoad(localUser.gameObject);
        onCreateLocalUser?.Invoke();
    }
}
