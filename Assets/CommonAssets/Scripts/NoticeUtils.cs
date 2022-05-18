using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class NoticeUtils : MonoBehaviour
{
    public static NoticeUtils ins;
    void Awake() { ins = this; }

    public bool internetCheck;
    
    public Transform alert;
    public Transform oneBtnAlert;
    public Transform twoBtnAlert;
    public Transform threeBtnAlert;
    public Transform inputAlert;
    public GameObject loadingAlert;
    public GameObject internetNotice;


    private Action<int> action;
    private Action<int,string,GameObject> inputAlertAction;
    private Action onLoadingTimeout;
    
    private int timeoutTime;

    private Coroutine AutoHideTwoBtnAlertCoroutine;

    void Start()
    {
        DontDestroyOnLoad(this);
        if (internetCheck) { StartCoroutine("InternetCheck"); }
    }

    IEnumerator InternetCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (!internetCheck) { yield break; }
            if (Application.internetReachability == NetworkReachability.NotReachable && !Application.isEditor)
            {internetNotice.SetActive(true);}
            else
            {internetNotice.SetActive(false);}
        }
    }

    public void ShowAlert(string notice,float disappearTime = 3)
    {
        alert.gameObject.SetActive(true);
        alert.GetChild(1).GetComponent<Text>().text = notice;
        StopCoroutine("ShowAlertCoroutine");
        StartCoroutine("ShowAlertCoroutine", disappearTime);
    }

    IEnumerator ShowAlertCoroutine(float disappearTime)
    {
        yield return new WaitForSeconds(disappearTime);
        alert.gameObject.SetActive(false);
    }

    public void ShowOneBtnAlert(string notice, Action<int> callBack = null, string btnTxt = "Ok")
    {
        oneBtnAlert.gameObject.SetActive(true);
        oneBtnAlert.GetChild(2).GetComponent<Text>().text = notice;
        oneBtnAlert.GetChild(3).GetChild(0).GetChild(1).GetComponent<Text>().text = btnTxt;
        if (callBack != null) { action = callBack; }

    }

    public void ShowTwoBtnAlert(string notice, Action<int> callBack, string firstBtnTxt = "Ok", string secondBtnTxt = "Cancel", float autoHideTime = 0, Action onAutoHide = null)
    {
        twoBtnAlert.gameObject.SetActive(true);
        twoBtnAlert.GetChild(2).GetComponent<Text>().text = notice;
        twoBtnAlert.GetChild(3).GetChild(0).GetChild(1).GetComponent<Text>().text = firstBtnTxt;
        twoBtnAlert.GetChild(3).GetChild(1).GetChild(1).GetComponent<Text>().text = secondBtnTxt;
        
        if (callBack != null) { action = callBack; }

        if (AutoHideTwoBtnAlertCoroutine != null) 
        { 
            StopCoroutine(AutoHideTwoBtnAlertCoroutine);
            AutoHideTwoBtnAlertCoroutine = null;
        }
        if (autoHideTime > 0) { AutoHideTwoBtnAlertCoroutine = StartCoroutine(AutoHideTwoBtnAlert(autoHideTime, onAutoHide)); }
    }

    public void HideTwoBtnAlert()
    {
        if (AutoHideTwoBtnAlertCoroutine != null)
        {
            StopCoroutine(AutoHideTwoBtnAlertCoroutine);
            AutoHideTwoBtnAlertCoroutine = null;
        }
        twoBtnAlert.gameObject.SetActive(false);
    }

    public void ShowThreeBtnAlert(string notice, Action<int> callBack, string firstBtnTxt = "Yes", string secondBtnTxt = "No",string thirdBtnTxt = "Close")
    {
        threeBtnAlert.gameObject.SetActive(true);
        threeBtnAlert.GetChild(2).GetComponent<Text>().text = notice;
        threeBtnAlert.GetChild(3).GetChild(0).GetChild(1).GetComponent<Text>().text = firstBtnTxt;
        threeBtnAlert.GetChild(3).GetChild(1).GetChild(1).GetComponent<Text>().text = secondBtnTxt;
        threeBtnAlert.GetChild(3).GetChild(2).GetChild(1).GetComponent<Text>().text = thirdBtnTxt;

        if (callBack != null) { action = callBack; }
    }

    public void ShowInputAlert(string notice, Action<int, string, GameObject> callBack, string placeholder = "Type here...", string firstBtnTxt = "Submit", string secondBtnTxt = "Close")
    {
        inputAlert.gameObject.SetActive(true);
        inputAlert.GetChild(2).GetComponent<Text>().text = notice;
        inputAlert.GetChild(3).GetChild(0).GetChild(1).GetComponent<Text>().text = firstBtnTxt;
        inputAlert.GetChild(3).GetChild(1).GetChild(1).GetComponent<Text>().text = secondBtnTxt;
        inputAlert.GetChild(4).GetComponent<InputField>().placeholder.GetComponent<Text>().text = placeholder;
        inputAlert.GetChild(4).GetComponent<InputField>().text = string.Empty;

        inputAlertAction = callBack; 
    }

    public void NotificationBtn(Transform btnTransform)
    {
        //GameUtils.ins.PlayBtnSound();

        string activeNotification = btnTransform.parent.parent.name;

        if (activeNotification == "InputAlert")
        {
            inputAlertAction?.Invoke(btnTransform.GetSiblingIndex(), inputAlert.GetChild(4).GetComponent<InputField>().text, inputAlert.gameObject);
        }
        else 
        {
            btnTransform.parent.parent.gameObject.SetActive(false);
            action?.Invoke(btnTransform.GetSiblingIndex());
            action = null;
        }
        

        if (activeNotification == "TwoBtnAlert" && AutoHideTwoBtnAlertCoroutine != null) { StopCoroutine(AutoHideTwoBtnAlertCoroutine); }
    }

    public GameObject ShowLoadingAlert(string notice, int timeoutTime = 15, Action onLoadingTimeout = null)
    {
        this.onLoadingTimeout = onLoadingTimeout;
        this.timeoutTime = timeoutTime;
        StopCoroutine("LoadingAlert");
        StartCoroutine("LoadingAlert", notice);
        return loadingAlert;
    }

    public void HideLoadingAlert()
    {
        StopCoroutine("LoadingAlert");
        loadingAlert.SetActive(false);
        onLoadingTimeout = null;
    }

    IEnumerator LoadingAlert(string notice)
    {
        float alertHideTime = timeoutTime;
        loadingAlert.SetActive(true);
        while (loadingAlert.activeInHierarchy && alertHideTime > 0)
        {
            string dots = "";
            for (int i = 0; i < 4; i++)
            {
                dots += ".";
                loadingAlert.transform.GetChild(2).GetComponent<Text>().text = notice + dots;
                alertHideTime -= .5f;
                yield return new WaitForSeconds(.5f);
            }
        }
        onLoadingTimeout?.Invoke();
        loadingAlert.SetActive(false);
    }

    IEnumerator AutoHideTwoBtnAlert(float autoHideTime, Action onAutoHide)
    {
        yield return new WaitForSeconds(autoHideTime);
        onAutoHide?.Invoke();
        HideTwoBtnAlert();
    }
}
