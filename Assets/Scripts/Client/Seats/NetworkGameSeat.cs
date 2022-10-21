using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGameSeat : MonoBehaviour
{
    public PlayerInfoUi playerInfo;
    public MoveAndBetInfoUI moveAndBetInfo;

    public Animation character;
    public Transform cameraPos;
    public GameObject chips;

    [Header("Assigned During Game -")]
    public int actorNo;
    public Player player;

    

    public Action onSeatOccupied;
    public Action onSeatVaccated;

    public void OccupySeat(int actorNo)
    {
        this.actorNo = actorNo;
        player = ph.GetPlayer(actorNo);

        moveAndBetInfo.bet.SetPotAmount(0);
        if (player.IsLocal)
        {
            name = "LocalPlayer";
            playerInfo = NetworkGameClient.ins.lpInfoUi;
            NetworkGameClient.ins.cam.position = cameraPos.position;
            NetworkGameClient.ins.cam.transform.GetChild(0).LookAt(NetworkGameClient.ins.camLookAtPoint);
        }
        else
        {
            character.gameObject.SetActive(true);
            character["SitIdle"].time = UnityEngine.Random.Range(0, character["SitIdle"].length);
        }

        playerInfo.SetUi(player);
        ServerClientBridge.ins.onServerMsgRecieved += OnServerMsgRecieved;
        onSeatOccupied?.Invoke();
    }


    public void OnServerMsgRecieved(ExitGames.Client.Photon.Hashtable properties)
    {
        if (properties["moveMadeBy"] != null && (int)properties["moveMadeBy"] == actorNo)
        {
            moveAndBetInfo.MakeMove((string)properties["moveMade"]);

            if (properties["moveAmount"] != null)
            {
                GameUtils.ins.PlaySound(NetworkGameClient.ins.betSound);
                chips.SetActive(true);
                moveAndBetInfo.MakeBet(playerInfo.balance, (float)properties["moveAmount"]);
            }
        }

        
    }





    public void VaccateSeat()
    {
        this.actorNo = 0;
        player = null;

        player = null;
        character.gameObject.SetActive(false);
        playerInfo.gameObject.SetActive(false);


        ServerClientBridge.ins.onServerMsgRecieved -= OnServerMsgRecieved;
        onSeatVaccated?.Invoke();
    }

    public int GetSeatIndex() { return NetworkGameClient.ins.seats.IndexOf(this); }
}
