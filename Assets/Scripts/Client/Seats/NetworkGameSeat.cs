using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGameSeat : MonoBehaviour
{
    public Pocket playerBalance;

    [HideInInspector] public NetworkRoomSeat networkRoomSeat;

    public float winAmount = 0;

    void Start()
    {
        networkRoomSeat = GetComponent<NetworkRoomSeat>();


        networkRoomSeat.onSeatOccupied += () =>
        {
            if (networkRoomSeat.player.IsLocal) { playerBalance = NetworkGameClient.ins.lpBalance; }
            playerBalance.SetAmount((float)ph.GetPlayerData(networkRoomSeat.player, "balance"));
        };

        networkRoomSeat.onSeatVaccated += () =>
        {
            playerBalance.Reset();
        };
    }

    public void AddBalance(float amount) { ChangeBalance(amount); }

    public void SubtractBalance(float amount) { ChangeBalance(-amount); }

    public void ChangeBalance(float amount)
    {
        winAmount += amount;

        playerBalance.AddAmount(amount);

        if (networkRoomSeat.actorNo > 0)
        {
            if (networkRoomSeat.player.IsLocal)
            {
                ph.ChangePlayerData(networkRoomSeat.player, "balance", amount);
                User.localUser.ChangeBalance(amount);
            }
        }
        else
        {
            if (ph.IsMasterClient())
            {
                ph.ChangePlayerData(networkRoomSeat.player, "balance", amount);
            }
        }
    }
}
