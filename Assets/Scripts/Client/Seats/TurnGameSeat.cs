using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnGameSeat : MonoBehaviour
{
    

    public uiLabel moveMade;
    

    public Timer timer;
    public Timer timer3D;

    public Action onTurnActive;

    [HideInInspector] public NetworkRoomSeat networkRoomSeat;
    [HideInInspector] public NetworkGameSeat networkGameSeat;

    void Start()
    {
        networkRoomSeat = GetComponent<NetworkRoomSeat>();
        networkGameSeat = GetComponent<NetworkGameSeat>();


        networkRoomSeat.onSeatOccupied += () =>
        {
            if (networkRoomSeat.player.IsLocal)
            {
                timer = TurnGameClient.ins.lpTimer;
            }
        };

    }
    
    public void StartTurn()
    {
        NetworkGameClient.ins.lpControls.SetActive(true);
        timer.StartTimer(TurnGameClient.ins.turnTime, null);
        timer3D.StartTimer(TurnGameClient.ins.turnTime, null);
    }

    public void MakeMove(string moveName, float amount = 0)
    {
        moveMade.SetLabel(moveName);

        if (amount > 0)
        {
            networkGameSeat.playerBalance.SubtractAmount(amount);
            ph.ChangePlayerData(networkRoomSeat.player, "balance", -amount);
        }
    }

    

    public void MoveMade()
    {
        timer.gameObject.SetActive(false);
        timer3D.gameObject.SetActive(false);
    }

    public void ResetMoveMade()
    {
        moveMade.Reset();
    }
}
