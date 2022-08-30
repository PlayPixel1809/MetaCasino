using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CasinoCollider : MonoBehaviour
{
    private void Update()
    {
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            Joystick.ins.onJoystickUp.Invoke();
            Joystick.ins.gameObject.SetActive(false);
            AnimUtils.FadeUi( GameUtils.ins.fadeSprite.transform, 0, 1, 1, ()=>
            {
                
                Room.ins.LeaveGameRoom(()=> 
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("PokerRoom_2");
                });
            });
        }
    }
}
