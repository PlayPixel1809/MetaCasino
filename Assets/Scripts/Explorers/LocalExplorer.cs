using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalExplorer : MonoBehaviour
{
    private TPSController tpsController;
    private float count;

    void Start()
    {
        tpsController = GetComponent<TPSController>();
        tpsController.onMove += OnCharacterMove;
        tpsController.onCharacterIdle += OnCharacterIdle;
    }
   
    void OnCharacterMove()
    {
        count += Time.deltaTime;
        if (count < .5f) { return; }
        count = 0;

        ExitGames.Client.Photon.Hashtable changedProp = new ExitGames.Client.Photon.Hashtable()
        {
            { "pos", transform.position},
            { "anim", GetCurrentPlayingAnimationClip(tpsController.anims)}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(changedProp);
    }

    void OnCharacterIdle()
    {
        ExitGames.Client.Photon.Hashtable changedProp = new ExitGames.Client.Photon.Hashtable()
        {
            { "pos",transform.position}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(changedProp);
    }


    public string GetCurrentPlayingAnimationClip(Animation animation)
    {
        foreach (AnimationState anim in animation)
        {
            if (animation.IsPlaying(anim.name))
            {
                return anim.name;
            }
            
        }
        return string.Empty;
    }

    
}
