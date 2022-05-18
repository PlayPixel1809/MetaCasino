using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCharacter : MonoBehaviourPunCallbacks
{
    public string nickname;
    public int actorNo;
    private SimpleCharacterController simpleCharacterController;

    void Start()
    {
        simpleCharacterController = GetComponent<SimpleCharacterController>();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer.ActorNumber != actorNo) { return; }


        //Debug.Log(targetPlayer.CustomProperties["anim"]);
        string animName = (string)targetPlayer.CustomProperties["anim"];
        Vector3 target = StringToVector3((string)targetPlayer.CustomProperties["pos"]);

        if (string.IsNullOrEmpty(animName)) 
        {
            Debug.Log("Only Got target");
            transform.LookAt(target);
            transform.position = target; 
        }
        
        if (animName == "Walk") { simpleCharacterController.Walk(target); }
        if (animName == "Run") { simpleCharacterController.Run(target); }
    }

    public static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }
}
