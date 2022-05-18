using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    public class UserInfo
    {
        public string nickname;
        public float balance = 100000;
        public Vector3 pos = Vector3.zero;
        public float rotY = 0;
    }
    
    
    public void CreateLocalUser(UserInfo userInfo)
    {
        PhotonNetwork.LocalPlayer.NickName = userInfo.nickname;
        PhotonNetwork.LocalPlayer.CustomProperties.Add("balance", userInfo.balance);
        PhotonNetwork.LocalPlayer.CustomProperties.Add("pos", userInfo.pos);
        PhotonNetwork.LocalPlayer.CustomProperties.Add("rotY", userInfo.rotY);
    }
}
