using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplorerManager : MonoBehaviour
{
    public static ExplorerManager ins;
    void Awake() { ins = this; }

    public TPSController localCharacter;
    public NetworkExplorer networkPlayerPrefab;

    public Transform playersInfoParent;


    void Start()
    {
        Room.ins.onRoomJoined += SpawnExplorers;
        Room.ins.onPlayerEnteredRoom += CreateNetworkCharacter;
        Room.ins.onPlayerLeftRoom += OnPlayerLeftLobby;
    }

    void SpawnExplorers(Player player)
    {
        localCharacter.gameObject.AddComponent<LocalExplorer>();
        localCharacter.transform.Translate(0, 0, (PhotonNetwork.PlayerList.Length - 1) * 5);
        ExitGames.Client.Photon.Hashtable changedProp = new ExitGames.Client.Photon.Hashtable()
        {
            { "pos", localCharacter.transform.position},
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(changedProp);


        for (int i = 0; i < PhotonNetwork.PlayerListOthers.Length; i++) { CreateNetworkCharacter(PhotonNetwork.PlayerList[i]); }
    }

    void CreateNetworkCharacter(Player player)
    {
        StartCoroutine(InstantiateWithPos(player));
    }

    void OnPlayerLeftLobby(Player player)
    {
        NetworkExplorer[] networkCharacters = FindObjectsOfType<NetworkExplorer>();
        for (int i = 0; i < networkCharacters.Length; i++)
        {
            if (networkCharacters[i].nickname == player.NickName) 
            {
                Destroy(networkCharacters[i].playerInfo.gameObject);
                Destroy(networkCharacters[i].gameObject); 
                break; 
            }
        }
    }

    IEnumerator InstantiateWithPos(Player player)
    {
        while (player.CustomProperties["pos"] == null) { yield return new WaitForSeconds(.1f); }

        NetworkExplorer networkExplorer = Instantiate(networkPlayerPrefab);
        networkExplorer.playerInfo.username.text = player.NickName;
        networkExplorer.transform.position = (Vector3)player.CustomProperties["pos"];
        networkExplorer.name = player.NickName;
        networkExplorer.character.walkSpeed = localCharacter.walkSpeed;
        networkExplorer.character.runSpeed = localCharacter.runSpeed;

        networkExplorer.nickname = player.NickName;
        networkExplorer.actorNo = player.ActorNumber;
    }
}
