using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlagSelectPanel : MonoBehaviour
{
    public Transform flagPrefab;
    public Transform flagsParent;

    public Action<Sprite> onFlagSelect;

    void Start()
    {
        NoticeUtils.ins.ShowLoadingAlert("Please Wait");
        Sprite[] flags = Resources.LoadAll<Sprite>("Flags");
        
        for (int i = 0; i < flags.Length; i++)
        {
            Transform newFlag = Instantiate(flagPrefab);
            newFlag.gameObject.SetActive(true);
            newFlag.name = flags[i].name;
            newFlag.parent = flagsParent;
            newFlag.localScale = Vector3.one;
            newFlag.GetChild(0).GetComponent<Image>().sprite = flags[i];
            newFlag.GetChild(0).GetComponent<Image>().SetNativeSize();
            newFlag.GetChild(1).GetComponent<Text>().text = flags[i].name;
        }
        NoticeUtils.ins.HideLoadingAlert();
    }

    public void FlagBtn(Transform btn)
    {
        gameObject.SetActive(false);
        onFlagSelect.Invoke(btn.GetChild(0).GetComponent<Image>().sprite);
    }

    public void ShowFlagSelectPanel(Action<Sprite> onFlagSelect)
    {
        gameObject.SetActive(true);
        this.onFlagSelect = onFlagSelect;
    }
}
