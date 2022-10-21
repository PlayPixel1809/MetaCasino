using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkRoom : MonoBehaviourPunCallbacks
{
    public static NetworkRoom ins;
    void Awake() { ins = this; }

    public float minBet = 1000;
    public float minBalance = 10000;

    public Action onRoomCreated;

    public Action<Player> onPlayerEnteredRoom;
    public Action<Player> onPlayerLeftRoom;

    public override void OnCreatedRoom()
    {
        onPlayerEnteredRoom?.Invoke(PhotonNetwork.LocalPlayer);
        onRoomCreated?.Invoke();
    }

    public override void OnPlayerEnteredRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient) { onPlayerEnteredRoom?.Invoke(otherPlayer); }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient) { onPlayerLeftRoom?.Invoke(otherPlayer); }
    }
}
