using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerClientBridge : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static ServerClientBridge ins;
    void Awake() { ins = this; }

    public Action<ExitGames.Client.Photon.Hashtable> onServerMsgRecieved;
    public Action<int, ExitGames.Client.Photon.Hashtable> onClientMsgRecieved;

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 0)
        {
            onServerMsgRecieved?.Invoke((ExitGames.Client.Photon.Hashtable)photonEvent.CustomData);
        }

        if (photonEvent.Code == 1)
        {
            onClientMsgRecieved?.Invoke(photonEvent.Sender, (ExitGames.Client.Photon.Hashtable)photonEvent.CustomData);
        }
    }

    public static void NotifyClients(string key, object val)
    {
        NotifyClients(new ExitGames.Client.Photon.Hashtable() { { key, val } });
    }

    public static void NotifyClients(ExitGames.Client.Photon.Hashtable data)
    {
        PhotonNetwork.RaiseEvent(0, data, new RaiseEventOptions() { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
    }

    public static void NotifyClient(int actorNo, string key, object val)
    {
        NotifyClient(actorNo, new ExitGames.Client.Photon.Hashtable() { { key, val } });
    }

    public static void NotifyClient(int actorNo, ExitGames.Client.Photon.Hashtable data)
    {
        PhotonNetwork.RaiseEvent(0, data, new RaiseEventOptions() { Receivers = ReceiverGroup.All, TargetActors = new int[1] { actorNo } }, SendOptions.SendReliable);
    }


    public static void NotifyServer(string key, object val)
    {
        NotifyServer(new ExitGames.Client.Photon.Hashtable() { { key, val } });
    }

    public static void NotifyServer(ExitGames.Client.Photon.Hashtable data)
    {
        PhotonNetwork.RaiseEvent(1, data, new RaiseEventOptions() { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
    }
}
