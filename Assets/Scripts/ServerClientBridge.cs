using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ServerClientBridge : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static ServerClientBridge ins;
    void Awake() { ins = this; }

    public string activeEvId;
    public bool evCompleted;

    public Action<string, Hashtable> onServerMsgRecieved;
    public Action<string, Hashtable> onClientMsgRecieved;
    public Action<int, string> onPhotonMsgRecieved;
    public Action onMadeMasterClient;

    private Hashtable completedEvData;
    private List<Player> joiningPlayers = new List<Player>();
    private List<Player> leavingPlayers = new List<Player>();

    public bool isMasterClient;

    void Update()
    {
        if (Application.isEditor && ph.IsMasterClient()) { isMasterClient = true; } else { isMasterClient = false; }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 0)
        {
            joiningPlayers.Clear();
            leavingPlayers.Clear();
            onServerMsgRecieved?.Invoke((string)((Hashtable)photonEvent.CustomData)["evId"], (Hashtable)photonEvent.CustomData);
        }

        if (photonEvent.Code == 1)
        {
            activeEvId = (string)((Hashtable)photonEvent.CustomData)["evId"];
            evCompleted = false;
            completedEvData = null;
            onServerMsgRecieved?.Invoke(activeEvId, (Hashtable)photonEvent.CustomData);
        }

        if (photonEvent.Code == 2)
        {
            NotifyServerIfMasterClient((string)((Hashtable)photonEvent.CustomData)["evId"], (Hashtable)photonEvent.CustomData);
        }
    }

    public override void OnCreatedRoom()
    {
        onPhotonMsgRecieved?.Invoke(PhotonNetwork.LocalPlayer.ActorNumber, "roomCreated");
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            onPhotonMsgRecieved?.Invoke(player.ActorNumber, "playerEnteredRoom");
        }
        else { joiningPlayers.Add(player); }
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            onPhotonMsgRecieved?.Invoke(player.ActorNumber, "playerLeftRoom");
        }
        else { leavingPlayers.Add(player); }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("OnMasterClientSwitched");

        if (newMasterClient == ph.GetLocalPlayer()) 
        { 
            onMadeMasterClient?.Invoke();

            for (int i = 0; i < joiningPlayers.Count; i++) { onPhotonMsgRecieved?.Invoke(joiningPlayers[i].ActorNumber, "playerEnteredRoom"); }
            for (int i = 0; i < leavingPlayers.Count; i++) { onPhotonMsgRecieved?.Invoke(leavingPlayers[i].ActorNumber, "playerEnteredRoom"); }
            
            if (completedEvData != null)    { ClientMsgRecieved(completedEvData); }
        }
    }

    public void NotifyClients(string evId, Hashtable data = null)
    {
        data = FormatData(evId, data, "notify clients");
        if (data != null) { PhotonNetwork.RaiseEvent(0, data, new RaiseEventOptions() { Receivers = ReceiverGroup.All }, SendOptions.SendReliable); }
    }
    public void NotifyClients(string evId, string key, object val) { NotifyClients(evId, new Hashtable() { { key, val } }); }



    public void HireClients(string evId, Hashtable data = null)
    {
        data = FormatData(evId, data, "hire clients");
        if (data != null) 
        {
            NetworkRoom.ins.SyncData("hireClientsEvData", data);
            PhotonNetwork.RaiseEvent(1, data, new RaiseEventOptions() { Receivers = ReceiverGroup.All }, SendOptions.SendReliable); 
        }
    }
    public void HireClients(string evId, string key, object val) { HireClients(evId, new Hashtable() { { key, val } });}
    public void InvokeLastHireCLientsEvent() 
    { 
        if (ph.GetRoomData("hireClientsEvData") != null) 
        {
            Debug.Log("InvokeLastHireCLientsEvent");
            Hashtable ev = (Hashtable)ph.GetRoomData("hireClientsEvData");
            onServerMsgRecieved?.Invoke((string)ev["evId"], ev); 
        } 
    }


    public void NotifyClient(int actorNo, string evId, Hashtable data = null)
    {
        data = FormatData(evId, data, "notify single client");
        if (data != null) { PhotonNetwork.RaiseEvent(0, data, new RaiseEventOptions() { Receivers = ReceiverGroup.All, TargetActors = new int[1] { actorNo } }, SendOptions.SendReliable); }
    }
    public void NotifyClient(int actorNo, string evId, string key, object val) { NotifyClient(actorNo, evId, new Hashtable() { { key, val } }); }


    
    public void NotifyServerIfMasterClient(string completedEvId, Hashtable data = null)
    {
        data = FormatData(completedEvId, data, "notify server");

        if (data != null && IsClientResponseValid(completedEvId))
        {
            if (ph.IsMasterClient())
            {
                ClientMsgRecieved(data);
            }
            else
            {
                completedEvData = data; 
            }
        }
    }
    public void NotifyServerIfMasterClient(string completedEvId, string key, object val) { NotifyServerIfMasterClient(completedEvId, new Hashtable() { { key, val } }); }



    public void NotifyServer(string completedEvId, Hashtable data = null)
    {
        data = FormatData(completedEvId, data, "notify server");

        if (data == null) { return; }

        if (ph.IsMasterClient()) { ClientMsgRecieved(data); }
        else
        {
            PhotonNetwork.RaiseEvent(2, data, new RaiseEventOptions() { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
        }
    }


    public Hashtable FormatData (string evId, Hashtable data, string actionType)
    {
        if (string.IsNullOrEmpty(evId)) { Debug.Log("Can't " + actionType + " with empty evId "); return null; }

        if (data == null) { data = new Hashtable() { { "evId", evId } }; }
        else
        {
            if (data.ContainsKey("evId"))
            {
                if ((string)data["evId"] != evId) 
                {
                    Debug.Log(actionType + " Data already contains evId '" + data["evId"] + "' replacing it with '" + evId + "'") ;
                    data["evId"] = evId;
                }
            }
            else { data.Add("evId", evId); }
        }
        return data;
    }

    public string GetLastHireClientsEvId()
    {
        if (ph.GetRoomData("hireClientsEvData") != null) {return (string)((Hashtable)ph.GetRoomData("hireClientsEvData"))["evId"]; }
        return string.Empty;
    }

    public bool MatchLastHireClientsEvId(string matchWith)
    {
        if ((string)((Hashtable)ph.GetRoomData("hireClientsEvData"))["evId"] == matchWith) { return true; } else { return false; }
    }

    void ClientMsgRecieved(Hashtable data)
    {
        if (string.IsNullOrEmpty((string)data["evId"])) { Debug.Log("Can't notify Server with empty evId "); return; }
        onClientMsgRecieved?.Invoke((string)data["evId"], data);
    }

    bool IsClientResponseValid(string completedEvId)
    {
        if (!evCompleted)
        {
            if (completedEvId == activeEvId)
            {
                evCompleted = true;
                return true;
            }
        }
        return false;
    }


    //There is server code and there is client code , server code is meant to run on online computer and client code is executed on players devices for eg mobile phones.
    //Online computer is simply a computer like we rent from amazon which is called AWS. This online computer is called server and when player opens the game in his device his device gets connected to this server and is refered as client. 
    //Suppose four players are playing poker with each other then we can say there are 4 clients connected to a common server.
    //since we want to save on cost of an online computer we have to run server code on one of the clients. 
    //The server code is present on all the clients devices but only one client will run the server code at any given time. That client is called masterclient.
    //Any client can quit playing at any given time in case masterclient quits the game, one of the remaining clients is made masterclient and so on.

    //server code is meant to be executed in a single frame or millisecond and dosent have waiting mechanism all the waiting is done on client side. examples will make it more clear -

    //1 - when second player connects to server , server will notify all the clients to start the game after 10 seconds counter. This 10 second wait is performed on all the clients and after 10 seconds clients
    //notify server that game counter has been finished and game can be started and table can be closed until game finishes.
    //2 -  when there is turn of any particular player server will notify all clients the turn index and turn time suppose 5 seconds. Suppose If active turn player dosent performs any action then After 5 seconds all clients try to notify
    //server that turn time has finished and appropriate action should be taken.

    //In case of second example , All the clients try to tell server that active player didnt made any move but only masterclient is allowed to notify server and non-masterclients record that data coz in case masterclient disconnects a
    //new client will be made masterclient and this new masterclient will notify server with the recorded data.

    //Suppose Active player responds with bet of 500 at 4 seconds , all the clients will get this message including masterclient and non-masterclients will record this message. suppose masterclient is not responding which is responsible
    //to notify all clients that active player has made a move so automatically 5 seconds timer (no move made check timer)  which is running on all the clients will not stop and they will try to notify server that no move is made.

    //since all these clients are non-masterclients so they will record this message too and override that 'bet of 500 message' which clearly is wrong. 
    //To overcome this we set recorded data only if its null. By this 500 bet message gets recorded and dosent gets over-rided by 'player didnt move message'. 
    //Also recorded data which we can call completedEvData gets nullified whenever we recieve any message from server.










}
