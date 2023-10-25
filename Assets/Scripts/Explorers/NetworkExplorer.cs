using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkExplorer : MonoBehaviourPunCallbacks
{
    public string nickname;
    public int actorNo;
    public SimpleCharacterController character;

    public Transform playerInfoAnchor;
    public PlayerInfoUi playerInfo;

    void Start()
    {
        playerInfo.transform.parent = ExplorerManager.ins.playersInfoParent;
        playerInfo.GetComponent<UiWorldAnchor>().anchorPoint = playerInfoAnchor;
        playerInfo.transform.localScale = Vector3.one;
        playerInfo.gameObject.SetActive(true);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer.ActorNumber != actorNo) { return; }

        //Debug.Log(targetPlayer.CustomProperties["anim"]);
        string animName = (string)targetPlayer.CustomProperties["anim"];
        Vector3 target = (Vector3)targetPlayer.CustomProperties["pos"];

        if (string.IsNullOrEmpty(animName)) 
        {
            Debug.Log("Only Got target");
            transform.LookAt(target);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            transform.position = target; 
        }
        
        if (animName == "Walk") { character.Walk(target); }
        if (animName == "Run") { character.Run(target); }
    }
    
}
