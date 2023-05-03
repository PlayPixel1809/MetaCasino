using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : MonoBehaviour
{
    public InteractionInfo casinoRightEntry;
    public InteractionInfo casinoLeftEntry;

    // Start is called before the first frame update
    void Start()
    {
        casinoRightEntry.onTriggerEnter += (c) => 
        {
            if (c.tag == "Player")
            {
                Joystick.ins.onJoystickUp.Invoke();
                Joystick.ins.gameObject.SetActive(false);
                AnimUtils.FadeUi(GameUtils.ins.fadeSprite.transform, 0, 1, 1, () =>
                {
                    Room.ins.LeaveGameRoom(() =>
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene("Casino");
                    });
                });
            }
        };

        casinoLeftEntry.onTriggerEnter += (c) =>
        {
            if (c.tag == "Player")
            {
                Joystick.ins.onJoystickUp.Invoke();
                Joystick.ins.gameObject.SetActive(false);
                AnimUtils.FadeUi(GameUtils.ins.fadeSprite.transform, 0, 1, 1, () =>
                {
                    Room.ins.LeaveGameRoom(() =>
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene("Casino");
                    });
                });
            }
        };
    }

    
}
