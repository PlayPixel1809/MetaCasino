using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnGameSeat : MonoBehaviour
{
    public PlayerInfoUi playerInfo;
    public MoveAndBetInfoUI moveAndBetInfo;
    public AudioClip betSound;

    public Animation character;
    public Transform cameraPos;
    public GameObject chips;
    public AnimationCurve betAnimCurve = AnimationCurve.Linear(0,0,1,1);
    public Timer timer;
    public Timer timer3D;

    public Player player;
    public Action onTurnActive;
    public Action onSeatOccupy;


    public void OccupySeat(Player player)
    {
        moveAndBetInfo.bet.SetPotAmount(0);
        this.player = player;

        if (player.IsLocal)
        {
            name = "LocalPlayer";
            playerInfo = TurnGame.ins.lpInfoUi;
            timer = TurnGame.ins.lpTimer;
            TurnGame.ins.cam.position = cameraPos.position;
            TurnGame.ins.cam.transform.GetChild(0).LookAt(TurnGame.ins.camLookAtPoint);
        }
        else
        {
            character.gameObject.SetActive(true);
            character["SitIdle"].time = UnityEngine.Random.Range(0, character["SitIdle"].length);
        }

        playerInfo.SetUi(player);
        Room.ins.onRoomPropertiesChanged += OnRoomPropertiesChanged;
       
        onSeatOccupy?.Invoke();
    }

    public void OnRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable properties)
    {
        if (properties["turn"] != null)
        {
            if ((int)properties["turn"] == GetSeatIndex())
            {
                if (player.IsLocal) { TurnGame.ins.lpControls.SetActive(true); }
                timer.StartTimer(TurnGame.ins.turnTime, null);
                timer3D.StartTimer(TurnGame.ins.turnTime, null);
            }
        }

        if (properties["moveMade"] != null)
        {
            if ((int)ph.GetRoomData("turn") == GetSeatIndex())
            {
                if (player.IsLocal) { TurnGame.ins.lpControls.SetActive(false); }
                timer.gameObject.SetActive(false);
                timer3D.gameObject.SetActive(false);

                moveAndBetInfo.MakeMove((string)properties["moveMade"]);
            }
        }

        if (properties["moveAmount"] != null)
        {
            if ((int)ph.GetRoomData("turn") == GetSeatIndex())
            {
                GameUtils.ins.PlaySound(betSound);
                chips.SetActive(true);
                moveAndBetInfo.MakeBet(playerInfo.balance, (float)properties["moveAmount"]);
            }
        }
    }


    public void VaccateSeat()
    {
        player = null;
        character.gameObject.SetActive(false);
        playerInfo.gameObject.SetActive(false);
    }

    public int GetSeatIndex() { return TurnGame.ins.turnGameSeats.IndexOf(this); }
}
