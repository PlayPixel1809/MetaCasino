using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkRoomSeat : MonoBehaviour
{
    public uiLabel playerName;
    public Animation character;
    public Transform cameraPos;

    [Header("Assigned During Game -")]
    public int actorNo;
    public Player player;

    

    public Action onSeatOccupied;
    public Action onSeatVaccated;

    [HideInInspector]
    public int seatIndex;

    private void Update()
    {
        if (!gameObject.activeInHierarchy) { Debug.Log("GotDeactivated"); }
    }

    public void OccupySeat(int actorNo)
    {
        this.actorNo = actorNo;
        seatIndex = GetSeatIndex();
        player = ph.GetPlayer(actorNo);
        //moveAndBetInfo.Reset();
        if (player.IsLocal)
        {
            name = "LocalPlayer";
            playerName = NetworkRoomClient.ins.lpName;

            NetworkRoomClient.ins.mainCam.transform.parent = cameraPos;
            NetworkRoomClient.ins.mainCam.transform.localPosition = Vector3.zero;
            NetworkRoomClient.ins.mainCam.transform.localEulerAngles = Vector3.zero;
        }
        else
        {
            character.gameObject.SetActive(true);
            character["SitIdle"].time = UnityEngine.Random.Range(0, character["SitIdle"].length);
        }

        playerName.SetLabel(ph.GetPlayerNickname(player));
        onSeatOccupied?.Invoke();
    }

   
    public void VaccateSeat()
    {
        if (player.IsLocal) 
        {
            if (HoldemTable.ins != null) { HoldemTable.ins.Back(); }
            if (OmahaTable.ins != null) { OmahaTable.ins.Back(); }
        }
        actorNo = 0;

        player = null;
        character.gameObject.SetActive(false);
        playerName.gameObject.SetActive(false);

        onSeatVaccated?.Invoke();
    }

    public int GetSeatIndex() { return NetworkRoomClient.ins.seats.IndexOf(this); }
}
