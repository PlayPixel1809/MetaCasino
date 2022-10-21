using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnGameSeat : MonoBehaviour
{
    public NetworkGameSeat networkGameSeat;


    public Timer timer;
    public Timer timer3D;

    public Action onTurnActive;

    void Start()
    {
        networkGameSeat.onSeatOccupied += () =>
        {
            if (networkGameSeat.player.IsLocal)
            {
                timer = TurnGameClient.ins.lpTimer;
            }

            ServerClientBridge.ins.onServerMsgRecieved += OnServerMsgRecieved;
        };

        networkGameSeat.onSeatVaccated += () =>
        {
            ServerClientBridge.ins.onServerMsgRecieved -= OnServerMsgRecieved;
        };
    }

    public void StartTurn()
    {
        if (networkGameSeat.player.IsLocal) { NetworkGameClient.ins.lpControls.SetActive(true); }
        timer.StartTimer(TurnGame.ins.turnTime, null);
        timer3D.StartTimer(TurnGame.ins.turnTime, null);
    }

    public void OnServerMsgRecieved(ExitGames.Client.Photon.Hashtable properties)
    {
        if (properties["turn"] != null)
        {
            if ((int)properties["turn"] == networkGameSeat.GetSeatIndex())
            {
                timer.StartTimer(TurnGame.ins.turnTime, null);
                timer3D.StartTimer(TurnGame.ins.turnTime, null);
            }
        }

        if (properties["moveMadeBy"] != null && (int)properties["moveMadeBy"] == networkGameSeat.actorNo)
        {
            timer.gameObject.SetActive(false);
            timer3D.gameObject.SetActive(false);
        }

        
    }


    


}
