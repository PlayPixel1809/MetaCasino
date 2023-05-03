using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Casino : MonoBehaviour
{
    public InteractionInfo casinoExit;

    // Start is called before the first frame update
    void Start()
    {
        casinoExit.onTriggerEnter += (c) =>
        {
            if (c.tag == "Player")
            {
                NoticeUtils.ins.ShowTwoBtnAlert("Are you sure you want to exit casino ", (i) =>
                {
                    if (i == 0)
                    {
                        Room.ins.LeaveGameRoom(() =>
                        {
                            UnityEngine.SceneManagement.SceneManager.LoadScene("City");
                        });
                    }
                });
            }
        };
    }
}
