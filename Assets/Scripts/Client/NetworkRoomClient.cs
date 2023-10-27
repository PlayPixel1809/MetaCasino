using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkRoomClient : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static NetworkRoomClient ins;
    void Awake() 
    { 
        ins = this; 
        expectedCustomRoomProperties = new List<KeyValue>(); 
    }

    public RoomOptions roomOptions;

    public string defaultEmail;
    public string defaultPassword;
    public bool loginInfoFromPlayerPrefs;

    public string roomName;
    public int maxPlayers;
    public int emptyRoomTtl;
    public int playerTtl;

    public List<KeyValue> customRoomProperties;
    public List<string> customRoomPropertiesForLobby;

    public Transform mainCam;

    public uiLabel lpName;
    public List<NetworkRoomSeat> seats;

    [Header("Assigned During Game -"), Space]
    public List<KeyValue> expectedCustomRoomProperties;

    public Action onConnectedToMaster;
    public Action onJoinRoom;
    public Action onSeatAssigned;
    public Action<ExitGames.Client.Photon.Hashtable> onRoomPropertiesChanged;
    public Action onRoomLeft;              // This runs when local player leaves current room

    public Action<string, string, string, string> onEvent;

    new void OnEnable() { PhotonNetwork.AddCallbackTarget(this); }
    new void OnDisable() { PhotonNetwork.RemoveCallbackTarget(this); }

    void Update()
    {
        //Debug.Log(PhotonNetwork.NetworkClientState);
        //if (Input.GetKey("a")) { AudioSource.PlayClipAtPoint(); }
        
    }

    void Start()
    {
        if (!string.IsNullOrEmpty(User.localUser.readyPlayerMeAvatarUrl)) 
        {
            ExitGames.Client.Photon.Hashtable changedProp = new ExitGames.Client.Photon.Hashtable()
            {
                { "readyPlayerMeAvatarUrl", User.localUser.readyPlayerMeAvatarUrl}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(changedProp);
        }
       

        if (User.localUser != null) { Initiate(); }
        else
        {
            User.onCreateLocalUser += Initiate;

            if (loginInfoFromPlayerPrefs)
            { User.LoginAndCreateLocalUser(PlayerPrefs.GetString("email"), PlayerPrefs.GetString("password")); }
            else
            { User.LoginAndCreateLocalUser(defaultEmail, defaultPassword); }
        }

        ServerClientBridge.ins.onServerMsgRecieved += OnMsgRecieved;
    }

    public void OnMsgRecieved(string evId, ExitGames.Client.Photon.Hashtable data)
    {
        if (evId == "PlayerEnteredRoom" || evId == "PlayerLeftRoom")
        {
            int[] modifiedSeats = (int[])data["seats"];
            for (int i = 0; i < seats.Count; i++)
            {
                if (modifiedSeats[i] != 0 && seats[i].actorNo == 0) 
                {
                    seats[i].OccupySeat(modifiedSeats[i]);
                    if (modifiedSeats[i] == ph.GetLocalPlayer().ActorNumber) { onSeatAssigned?.Invoke(); }
                }
                if (modifiedSeats[i] == 0 && seats[i].actorNo != 0) { seats[i].VaccateSeat(); }
            }
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
        onJoinRoom?.Invoke();

        NoticeUtils.ins.ShowLoadingAlert("Joining Random Room");

        if (string.IsNullOrEmpty(roomName))
        {
            PhotonNetwork.JoinRandomRoom(KeyValue.GetHashtableFromKeyValueList(expectedCustomRoomProperties), Convert.ToByte(maxPlayers));
        }
        else
        { 
            PhotonNetwork.JoinRoom(roomName); 
        }
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
        NoticeUtils.ins.HideLoadingAlert();
    }

    public void CreateRoom()
    {
        Debug.Log("CreatingRoom");
        NoticeUtils.ins.ShowLoadingAlert("CREATING ROOM, PLEASE WAIT ");

        RoomOptions roomOptions = new RoomOptions();

        roomOptions.MaxPlayers = Convert.ToByte(maxPlayers); 
        roomOptions.EmptyRoomTtl = 3000;
        roomOptions.PlayerTtl = 3000;

        roomOptions.CustomRoomProperties = KeyValue.GetHashtableFromKeyValueList(customRoomProperties);
        roomOptions.CustomRoomPropertiesForLobby = customRoomPropertiesForLobby.ToArray();

        PhotonNetwork.CreateRoom(roomName, roomOptions);
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
            /*Dictionary<byte, object> data = (Dictionary<byte, object>)photonEvent.CustomData;
            string evSender = ph.GetUsername(photonEvent.Sender);
            string evClass = (string)data[0];
            string evName = (string)data[1];
            string evData = (string)data[2];
            onEvent?.Invoke(evSender, evClass, evName, evData);
            Debug.Log("Event Recieved: evSender: " + evSender + ", evName: " + evName);*/
        }
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

    
    public static void CloseRoom()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }
}
