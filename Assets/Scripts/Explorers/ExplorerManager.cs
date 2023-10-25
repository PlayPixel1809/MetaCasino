using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using ReadyPlayerMe.Core;
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
        ExitGames.Client.Photon.Hashtable changedProp = new ExitGames.Client.Photon.Hashtable()
        {
            { "pos", localCharacter.transform.position}
        };
        if (!string.IsNullOrEmpty(User.localUser.readyPlayerMeAvatarUrl)) { changedProp.Add("readyPlayerMeAvatarUrl", User.localUser.readyPlayerMeAvatarUrl); }
        if (!Application.isEditor) { changedProp.Add("readyPlayerMeAvatarUrl", "https://models.readyplayer.me/649c0c1ea78d60625aa071a8.glb"); }

        PhotonNetwork.LocalPlayer.SetCustomProperties(changedProp);

        if (User.localUser.readyPlayerMeAvatar != null) 
        {
            Transform avatar = Instantiate(User.localUser.readyPlayerMeAvatar.transform, localCharacter.transform.position, Quaternion.identity, localCharacter.transform);
            avatar.gameObject.SetActive(true);
            avatar.localEulerAngles = Vector3.zero;
            localCharacter.anims.gameObject.SetActive(false);
            avatar.GetComponent<Animator>().runtimeAnimatorController = localCharacter.animator.runtimeAnimatorController;
            localCharacter.animator = avatar.GetComponent<Animator>();
        }

        Room.ins.onRoomJoined += SpawnExplorers;
        Room.ins.onPlayerEnteredRoom += CreateNetworkCharacter;
        Room.ins.onPlayerLeftRoom += OnPlayerLeftLobby;
    }

    public void GetBalanceBtn()
    {
        User.localUser.ChangeBalance(50000, PlayerInfoPanel.ins.SetInfo, "Please Wait..");
    }

    void SpawnExplorers(Player player)
    {
        localCharacter.gameObject.AddComponent<LocalExplorer>();
        localCharacter.transform.Translate(0, 0, (PhotonNetwork.PlayerList.Length - 1) * 5);
        


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

        if (player.CustomProperties["readyPlayerMeAvatarUrl"] != null)
        {
            var avatarLoader = new AvatarObjectLoader();
            avatarLoader.OnCompleted += (_, args) =>
            {
                GameObject avatar = new GameObject();
                avatar = args.Avatar;
                AvatarAnimatorHelper.SetupAnimator(args.Metadata.BodyType, avatar);

                avatar.transform.parent = networkExplorer.transform;
                avatar.transform.localPosition = Vector3.zero;
                avatar.transform.localEulerAngles = Vector3.zero;
                networkExplorer.character.character.gameObject.SetActive(false);
                avatar.GetComponent<Animator>().runtimeAnimatorController = networkExplorer.character.characterAnimator.runtimeAnimatorController;
                networkExplorer.character.characterAnimator = avatar.GetComponent<Animator>();
            };
            avatarLoader.LoadAvatar((string)player.CustomProperties["readyPlayerMeAvatarUrl"]);
            //if (Application.isEditor) { avatarLoader.LoadAvatar((string)player.CustomProperties["readyPlayerMeAvatarUrl"]); }
        }
    }
}
