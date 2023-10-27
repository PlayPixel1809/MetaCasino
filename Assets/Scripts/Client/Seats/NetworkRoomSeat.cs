using Photon.Realtime;
using ReadyPlayerMe.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkRoomSeat : MonoBehaviour
{
    public uiLabel playerName;
    public Animation characterAnimation;
    public Animator characterAnimator;
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
            if (player.CustomProperties["readyPlayerMeAvatarUrl"] == null)
            {
                characterAnimation.gameObject.SetActive(true);
                characterAnimation["SitIdle"].time = UnityEngine.Random.Range(0, characterAnimation["SitIdle"].length);
            }
            else 
            {
                var avatarLoader = new AvatarObjectLoader();
                avatarLoader.OnCompleted += (_, args) =>
                {
                    GameObject avatar = new GameObject();
                    avatar = args.Avatar;
                    AvatarAnimatorHelper.SetupAnimator(args.Metadata.BodyType, avatar);
                    avatar.GetComponent<Animator>().applyRootMotion = false;
                    avatar.transform.parent = transform;
                    avatar.transform.localPosition = new Vector3(0,.08f,.04f);
                    avatar.transform.localEulerAngles = Vector3.zero;
                    avatar.GetComponent<Animator>().runtimeAnimatorController = characterAnimator.runtimeAnimatorController;
                    characterAnimator = avatar.GetComponent<Animator>();
                    characterAnimator.Play("Sit", 0, UnityEngine.Random.Range(0, 1));
                };
                avatarLoader.LoadAvatar((string)player.CustomProperties["readyPlayerMeAvatarUrl"]);
            }
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
        characterAnimation.gameObject.SetActive(false);
        characterAnimator.gameObject.SetActive(false);
        playerName.gameObject.SetActive(false);

        onSeatVaccated?.Invoke();
    }

    public int GetSeatIndex() { return NetworkRoomClient.ins.seats.IndexOf(this); }
}
