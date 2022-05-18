using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonInitializer : MonoBehaviourPunCallbacks
{
    public static PhotonInitializer ins;
    public void Awake() { ins = this; }

    public bool initializeSilently;
    public Action onPhotonLobbyJoined;

    void Start()
    {
        if (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer) 
        {
            PhotonNetwork.ConnectUsingSettings();
            if (!initializeSilently) { NoticeUtils.ins.ShowLoadingAlert("Connecting to server"); }
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to photon master server, Joining Photon Lobby");
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Could not connect to photon master server cause " + cause.ToString() + " , Trying again");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Photon Lobby Joined");
        onPhotonLobbyJoined?.Invoke();
        NoticeUtils.ins.HideLoadingAlert();
    }
}
