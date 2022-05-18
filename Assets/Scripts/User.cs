using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour
{
    public enum UserTypes { Local, Network, Bot }

    public static User localUser;

    [HideInInspector] public UserTypes userType;

    public class UserInfo
    {
        public string nickname;
        public float balance = 100000;
        public Vector3 pos = Vector3.zero;
        public float rotY = 0;
    }


    public void MakeLocal(UserInfo userInfo)
    {
        gameObject.SetActive(true);
        userType = UserTypes.Local;
        localUser = this;
        gameObject.AddComponent<UserSceneData>();

        PhotonNetwork.LocalPlayer.NickName = userInfo.nickname;
        PhotonNetwork.LocalPlayer.CustomProperties.Add("balance", userInfo.balance);
        //PhotonNetwork.LocalPlayer.CustomProperties.Add("pos", (Vector3.zero).ToString());


        DontDestroyOnLoad(this);
    }

    public static void CreateLocalUser(UserInfo userInfo, Action<Player> onUserCreated)
    {
        User user = new GameObject("LocalUser").AddComponent<User>();
        user.MakeLocal(userInfo);
        onUserCreated?.Invoke(PhotonNetwork.LocalPlayer);
    }

    public static void CreateLocalUser(Action<Player> onUserCreated)
    {
        NoticeUtils.ins.ShowInputAlert("Please enter your name", (i, s, g) =>
        {
            if (i == 0)
            {
                if (string.IsNullOrEmpty(s)) { NoticeUtils.ins.ShowOneBtnAlert("Name cannot be empty"); return; }
                
                g.SetActive(false);
                CreateLocalUser(new UserInfo() { nickname = s }, onUserCreated);
            }
        });
    }
}
