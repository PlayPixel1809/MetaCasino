using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarSelectPanel : MonoBehaviour
{
    public Transform avatarPrefab;
    public Transform avatarsParent;

    public Action<Sprite> onAvatarSelect;

    private Sprite[] avatars;

    void Start()
    {
        NoticeUtils.ins.ShowLoadingAlert("Please Wait");

        avatars = Resources.LoadAll<Sprite>("Avatars");

        for (int i = 0; i < avatars.Length; i++)
        {
            Transform newAvatar = Instantiate(avatarPrefab);
            newAvatar.gameObject.SetActive(true);
            newAvatar.name = avatars[i].name;
            newAvatar.parent = avatarsParent;
            newAvatar.localScale = Vector3.one;
            newAvatar.GetChild(0).GetComponent<Image>().sprite = avatars[i];
        }
        NoticeUtils.ins.HideLoadingAlert();

    }

    public void AvatarBtn(Transform btn)
    {
        gameObject.SetActive(false);
        onAvatarSelect?.Invoke(btn.GetChild(0).GetComponent<Image>().sprite);
    }

    public void ShowAvatarSelectPanel(Action<Sprite> onAvatarSelect)
    {
        gameObject.SetActive(true);
        this.onAvatarSelect = onAvatarSelect;
    }
}
