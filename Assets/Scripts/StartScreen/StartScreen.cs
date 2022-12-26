using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class StartScreen : MonoBehaviour
{
    public static StartScreen ins;
    void Awake() { ins = this; }

    public GameObject loginPanel;
    public GameObject signupPanel;

    public Transform loginBtn;
    public Transform registerBtn;

    void Start()
    {
        if (User.localUser != null) { DestroyImmediate(User.localUser.gameObject); }
    }


    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            NoticeUtils.ins.ShowTwoBtnAlert("Do you want to exit the game ?", (i) =>
            {
                if (i == 0) { Application.Quit(); }
            });
        }
    }

    public void LoginPanelBtn()
    {
        signupPanel.SetActive(false);
        loginPanel.SetActive(true);
        loginBtn.GetChild(0).gameObject.SetActive(false);
        registerBtn.GetChild(0).gameObject.SetActive(true);
    }

    public void SignupPanelBtn()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(true);
        loginBtn.GetChild(0).gameObject.SetActive(true);
        registerBtn.GetChild(0).gameObject.SetActive(false);
    }

    
}
