using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldemTable : MonoBehaviour
{
    public static HoldemTable ins;
    void Awake() { ins = this; }

    public void BackBtn()
    {
        NoticeUtils.ins.ShowTwoBtnAlert("Are you sure you want to quit the game?", (i) =>
        {
            if (i == 0) { Back(); }
        });
    }

    public void Back()
    {
        NetworkRoomClient.ins.LeaveGameRoom(() => { UnityEngine.SceneManagement.SceneManager.LoadScene("Casino"); }); 
    }
}
