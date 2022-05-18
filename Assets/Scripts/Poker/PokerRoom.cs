using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerRoom : MonoBehaviourPunCallbacks
{
    public static PokerRoom ins;
    public void Awake() { ins = this; }

    public Action onRoomJoined;
    public Action<Player> onPlayerEnteredRoom;
    public Action<Player> onPlayerLeftRoom;
    private Action onLeaveRoom;

    new void OnEnable() { PhotonNetwork.AddCallbackTarget(this); }
    new void OnDisable() { PhotonNetwork.RemoveCallbackTarget(this); }

    void Start()
    {
        if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer) { JoinPokerRoom(); }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
            NoticeUtils.ins.ShowLoadingAlert("Connecting to server");
        }
    }

    public override void OnConnectedToMaster()
    {
        NoticeUtils.ins.HideLoadingAlert();
        Debug.Log("Connected to photon master server, Joining Poker Room");
        JoinPokerRoom();
    }

    public void JoinPokerRoom()
    {
        if (User.localUser == null)
        {
            User.CreateLocalUser((u) => { JoinPokerRoom(); });
            return;
        }

        NoticeUtils.ins.ShowLoadingAlert("JOINING POKER ROOM, PLEASE WAIT");
        Debug.Log("JoiningPokerRoomRoom");
        PhotonNetwork.JoinRoom("StreetEnv");
    }

    public override void OnJoinedRoom()
    {
        NoticeUtils.ins.HideLoadingAlert();
        Debug.Log("PokerRoomRoomJoined");
        onRoomJoined?.Invoke();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("JoiningPokerRoomRoomFailed, CreatingPokerRoomRoom");
        NoticeUtils.ins.ShowLoadingAlert("CREATING POKER ROOM, PLEASE WAIT ");
        PhotonNetwork.CreateRoom("PokerRoom");
    }

    
    public override void OnPlayerEnteredRoom(Player newPlayer) { onPlayerEnteredRoom?.Invoke(newPlayer); }
    public override void OnPlayerLeftRoom(Player otherPlayer) { onPlayerLeftRoom?.Invoke(otherPlayer); }

    public void LeaveLobby(Action onLeaveLobby)
    {
        NoticeUtils.ins.ShowLoadingAlert("Leaving Poker Room");
        Debug.Log("PokerRoom");
        PhotonNetwork.LeaveRoom();
        this.onLeaveRoom = onLeaveLobby;
    }

    public override void OnLeftRoom()
    {
        NoticeUtils.ins.HideLoadingAlert();
        Debug.Log("OnLeftPokerRoom");
        onLeaveRoom?.Invoke();
        onLeaveRoom = null;
    }
}
