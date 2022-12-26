﻿using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static Room ins;
    void Awake() { ins = this; }

    public string defaultEmail;
    public string defaultPassword;
    public bool loginInfoFromPlayerPrefs;

    public string roomName;
    public List<KeyValue> initialProperties;
    public List<string> matchmakingProperties;
    public int maxPlayers = 5;
    public float minBet = 1000;
    public float minBalance = 10000;

    public ExitGames.Client.Photon.Hashtable startProperties = new ExitGames.Client.Photon.Hashtable();

    public Action onConnectedToMaster;
    public Action onRoomCreated;
    public Action<Player> onRoomJoined;
    public Action<ExitGames.Client.Photon.Hashtable> onRoomPropertiesChanged;
    public Action onRoomLeft;              // This runs when local player leaves current room
    public Action<Player> onPlayerEnteredRoom;
    public Action<Player> onPlayerLeftRoom; // This runs when some other player leaves the room
    
    public Action<string, string, string, string> onEvent;

    new void OnEnable() { PhotonNetwork.AddCallbackTarget(this); }
    new void OnDisable() { PhotonNetwork.RemoveCallbackTarget(this); }

    void Update()
    {
        //Debug.Log(PhotonNetwork.NetworkClientState);
    }

    void Start()
    {
        AddPropertiesToRoom(initialProperties);

        if (User.localUser != null) { Initiate(); }
        else
        {
            User.onCreateLocalUser += Initiate;
             
            if (loginInfoFromPlayerPrefs) 
            { User.LoginAndCreateLocalUser(PlayerPrefs.GetString("email"), PlayerPrefs.GetString("password")); }
            else
            { User.LoginAndCreateLocalUser(defaultEmail, defaultPassword); }
        }
    }

    void Initiate()
    {
        if (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer)
        {
            PhotonNetwork.ConnectUsingSettings();
            NoticeUtils.ins.ShowLoadingAlert("Connecting to photon master server");
        }
        else { Utils.FrameDelayedAction(JoinRoom); }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to photon master server");
        NoticeUtils.ins.HideLoadingAlert();
        if (PhotonNetwork.CurrentRoom != null) { Debug.Log("PhotonNetwork.CurrentRoom"); }
        onConnectedToMaster?.Invoke();
        JoinRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Could not connect to photon master server cause " + cause.ToString() + " , Trying again");
        NoticeUtils.ins.ShowLoadingAlert("Reconnecting to previous room");
        PhotonNetwork.ReconnectAndRejoin();
    }
    
    public void JoinRoom()
    {
        Debug.Log("JoiningRandomRoom");
        NoticeUtils.ins.ShowLoadingAlert("Joining Random Room");

        ExitGames.Client.Photon.Hashtable expectedProps = new ExitGames.Client.Photon.Hashtable();
        for (int i = 0; i < matchmakingProperties.Count; i++)
        {
            expectedProps.Add(matchmakingProperties[i], startProperties[matchmakingProperties[i]]);
        }

        PhotonNetwork.JoinRandomRoom(expectedProps, Convert.ToByte(maxPlayers));
    }

    public override void OnJoinRoomFailed(short returnCode, string message) 
    {
        Debug.Log("OnJoinRoomFailed");
        
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed");
        NoticeUtils.ins.ShowLoadingAlert("Creating room, please wait ");
        CreateRoom();
    }

    public override void OnJoinedRoom() 
    {
        Debug.Log("OnJoinedRoom");
        NoticeUtils.ins.HideLoadingAlert();
        onRoomJoined?.Invoke(PhotonNetwork.LocalPlayer);
    }

    public void CreateRoom()
    {
        Debug.Log("CreatingRoom");
        NoticeUtils.ins.ShowLoadingAlert("CREATING ROOM, PLEASE WAIT ");

        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 2 };

        startProperties.Add("status",string.Empty);
        startProperties.Add("data", string.Empty);
        roomOptions.CustomRoomPropertiesForLobby = matchmakingProperties.ToArray();
        roomOptions.CustomRoomProperties = startProperties;
        if (maxPlayers > 0) { roomOptions.MaxPlayers = Convert.ToByte(1); }
        roomOptions.EmptyRoomTtl = 3000;
        roomOptions.PlayerTtl = 3000;

        PhotonNetwork.CreateRoom(string.Empty, roomOptions);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("RoomCreated");
        onRoomCreated?.Invoke();
    }

    public void LeaveGameRoom(Action onRoomLeft) 
    {
        this.onRoomLeft = onRoomLeft; 
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom() 
    {
        ph.RemovePlayerData(PhotonNetwork.LocalPlayer);
        onRoomLeft?.Invoke(); 
    }

    public static void SendEvent(string evClass, string evName, string data = "") 
    {
        Dictionary<byte, object> dictionary = new Dictionary<byte, object>() { { 0, evClass }, { 1, evName }, { 2, data } };
        
        PhotonNetwork.RaiseEvent(0, dictionary, RaiseEventOptions.Default, SendOptions.SendReliable);
        Debug.Log("Event Sent: evName: " + evName + ", Data: " + data);
    }

    public static void SendEvent(string evName, string data = "") { SendEvent("", evName, data); }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 0)
        {
            Dictionary<byte, object> data = (Dictionary<byte, object>)photonEvent.CustomData;
            string evSender = ph.GetUsername(photonEvent.Sender);
            string evClass = (string)data[0];
            string evName  = (string)data[1];
            string evData  = (string)data[2];
            onEvent?.Invoke(evSender, evClass, evName, evData);
            Debug.Log("Event Recieved: evSender: " + evSender + ", evName: " + evName);
        }
    }

    public override void OnPlayerEnteredRoom(Player otherPlayer) 
    { 
        onPlayerEnteredRoom?.Invoke(otherPlayer);
        onEvent?.Invoke(string.Empty, otherPlayer.NickName, "PlayerEnteredRoom", string.Empty);
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer) 
    { 
        onPlayerLeftRoom?.Invoke(otherPlayer);
        onEvent?.Invoke(string.Empty, otherPlayer.NickName, "PlayerLeftRoom", string.Empty);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) 
    { 
        onRoomPropertiesChanged?.Invoke(propertiesThatChanged); 
    }

    public static bool IsConnectedToMaster()
    {
        if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer) { return true; }
        return false;
    }

    public void AddPropertiesToRoom(List<KeyValue> properties)
    {
        for (int i = 0; i < properties.Count; i++)
        {
            if (PhotonNetwork.CurrentRoom == null)
            { startProperties.Add(properties[i].key, properties[i].GetVal()); }
            else
            { ph.SetRoomData(properties[i].key, properties[i].GetVal()); }
        }
    }

    public ExitGames.Client.Photon.Hashtable GetRoomProperties()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            return PhotonNetwork.CurrentRoom.CustomProperties;
        }
        else
        { return startProperties; }
    }
}
