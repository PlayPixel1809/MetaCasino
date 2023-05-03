using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEntrance : MonoBehaviour
{
    public string gameName;
    public bool confirmEntry = true;
    public float minBalance = 10000;
    public string sceneToLoad;


    public GameObject enterBtn;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            enterBtn.SetActive(true);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Player")
        {
            enterBtn.SetActive(false);
        }
    }

    public void EnterBtn()
    {
        if (confirmEntry)
        {
            NoticeUtils.ins.ShowTwoBtnAlert("Are you sure you want to play " + gameName, (i) =>
            {
                if (i == 0)
                {
                    if (CheckBalance())
                    {
                        Joystick.ins.onJoystickUp.Invoke();
                        Joystick.ins.gameObject.SetActive(false);
                        AnimUtils.FadeUi(GameUtils.ins.fadeSprite.transform, 0, 1, 1, () =>
                        {

                            Room.ins.LeaveGameRoom(() =>
                            {
                                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
                            });
                        });
                    }
                }
            });
        }
    }

    bool CheckBalance()
    {
        if (User.localUser.balance < minBalance)
        {
            NoticeUtils.ins.ShowOneBtnAlert("You Balance Is Low, Minimum " + minBalance + " balance Required");
            return false;
        }
        return true;
    }

}
