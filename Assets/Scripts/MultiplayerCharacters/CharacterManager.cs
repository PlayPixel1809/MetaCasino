using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public TPSController localCharacter;

    public SimpleCharacterController characterPrefab;

    void Start()
    {
        StreetEnv.ins.onLobbyJoined += CreateCharacters;
        StreetEnv.ins.onPlayerEnteredLobby += CreateNetworkCharacter;
        StreetEnv.ins.onPlayerLeftLobby += OnPlayerLeftLobby;
    }

    void CreateCharacters()
    {
        localCharacter.gameObject.AddComponent<LocalCharacter>();
        localCharacter.transform.Translate(0, 0, (PhotonNetwork.PlayerList.Length - 1) * 5);
        ExitGames.Client.Photon.Hashtable changedProp = new ExitGames.Client.Photon.Hashtable()
        {
            { "pos", localCharacter.transform.position.ToString()},
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(changedProp);
        localCharacter.nameTxt.text = PhotonNetwork.LocalPlayer.NickName;


        for (int i = 0; i < PhotonNetwork.PlayerListOthers.Length; i++) { CreateNetworkCharacter(PhotonNetwork.PlayerList[i]); }
    }

    void CreateNetworkCharacter(Player player)
    {
        StartCoroutine(InstantiateWithPos(player));
    }

    void OnPlayerLeftLobby(Player player)
    {
        NetworkCharacter[] networkCharacters = FindObjectsOfType<NetworkCharacter>();
        for (int i = 0; i < networkCharacters.Length; i++)
        {
            if (networkCharacters[i].nickname == player.NickName) 
            { 
                DestroyImmediate(networkCharacters[i].gameObject); 
                break; 
            }
        }
    }

    IEnumerator InstantiateWithPos(Player player)
    {
        while (player.CustomProperties["pos"] == null) { yield return new WaitForSeconds(.1f); }

        SimpleCharacterController character = Instantiate(characterPrefab);
        character.transform.position = NetworkCharacter.StringToVector3((string)player.CustomProperties["pos"]); 
        character.nameTxt.text = player.NickName;
        character.name = player.NickName;
        character.walkSpeed = localCharacter.walkSpeed;
        character.runSpeed = localCharacter.runSpeed;

        NetworkCharacter networkCharacter = character.gameObject.AddComponent<NetworkCharacter>();
        networkCharacter.nickname = player.NickName;
        networkCharacter.actorNo = player.ActorNumber;
    }
}
