using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGameSeat : MonoBehaviour
{
    public Pocket playerBalance;

    [HideInInspector] public NetworkRoomSeat networkRoomSeat;

    void Start()
    {
        networkRoomSeat = GetComponent<NetworkRoomSeat>();


        networkRoomSeat.onSeatOccupied += () =>
        {
            if (networkRoomSeat.player.IsLocal)
            {
                playerBalance = NetworkGameClient.ins.lpBalance;
            }

            playerBalance.AddAmount((float)ph.GetPlayerData(networkRoomSeat.player, "balance"));
        };

        networkRoomSeat.onSeatVaccated += () =>
        {
            playerBalance.gameObject.SetActive(false);
        };
    }
    
}
