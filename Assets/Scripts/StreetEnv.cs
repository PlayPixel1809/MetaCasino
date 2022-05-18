using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetEnv : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static StreetEnv ins;
    public void Awake() { ins = this; }

    public Action onLobbyJoined;
    public Action<Player> onPlayerEnteredLobby;
    public Action<Player> onPlayerLeftLobby;
    private Action onLeaveLobby;

    new void OnEnable()  { PhotonNetwork.AddCallbackTarget(this); }
    new void OnDisable() { PhotonNetwork.RemoveCallbackTarget(this); }

    void Start()
    {
        if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer) { JoinStreetEnv(); }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
            NoticeUtils.ins.ShowLoadingAlert("Connecting to server"); 
        }
    }

    public override void OnConnectedToMaster()
    {
        NoticeUtils.ins.HideLoadingAlert();
        Debug.Log("Connected to photon master server, Joining Street Env");
        JoinStreetEnv();
    }

    public void JoinStreetEnv()
    {
        if (User.localUser == null)
        {
            User.CreateLocalUser((u) => { JoinStreetEnv(); });
            return;
        }

        NoticeUtils.ins.ShowLoadingAlert("JOINING LOBBY, PLEASE WAIT");
        Debug.Log("JoiningStreetEnvRoom");
        PhotonNetwork.JoinRoom("StreetEnv");
    }

    public override void OnJoinedRoom()
    {
        NoticeUtils.ins.HideLoadingAlert();
        Debug.Log("StreetEnvRoomJoined");
        onLobbyJoined?.Invoke();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("JoiningStreetEnvRoomFailed, CreatingStreetEnvRoom");
        NoticeUtils.ins.ShowLoadingAlert("CREATING LOBBY, PLEASE WAIT ");
        PhotonNetwork.CreateRoom("StreetEnv");
    }

    public void OnEvent(EventData photonEvent)
    {
        
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        onPlayerEnteredLobby?.Invoke(newPlayer);
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        onPlayerLeftLobby?.Invoke(otherPlayer);
    }

    public void LeaveLobby(Action onLeaveLobby)
    {
        NoticeUtils.ins.ShowLoadingAlert("Leaving Lobby");
        Debug.Log("LeaveLobby");
        PhotonNetwork.LeaveRoom();
        this.onLeaveLobby = onLeaveLobby;
    }

    public override void OnLeftRoom()
    {
        NoticeUtils.ins.HideLoadingAlert();
        Debug.Log("OnLeftRoom");
        onLeaveLobby?.Invoke();
        onLeaveLobby = null;
    }
}
