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
                NetworkRoomClient.ins.mainCam.LookAt(TurnGameClient.ins.camLookAtPoint);
            }
        };

        networkRoomSeat.onSeatVaccated += () =>
        {
            ResetMoveMade();
            StopTurn();
        };
    }
    
    public void ExecuteTurn()
    {
        if (networkRoomSeat.player.IsLocal) { NetworkGameClient.ins.lpControls.SetActive(true); }
        timer.StartTimer(TurnGameClient.ins.turnTime, null);
    }

    public void StopTurn()
    {
        NetworkGameClient.ins.lpControls.SetActive(false); 
        timer.gameObject.SetActive(false);
    }

    public void MakeMove(string moveName, float amount = 0)
    {
        moveMade.SetLabel(moveName);

        if (amount > 0) { networkGameSeat.SubtractBalance(amount); }
    }

    

    public void MoveMade()
    {
        timer.gameObject.SetActive(false);
    }

    public void ResetMoveMade()
    {
        moveMade.Reset();
    }

    
}
