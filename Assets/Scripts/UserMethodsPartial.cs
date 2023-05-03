using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using Newtonsoft.Json;

public partial class User 
{
    public void NewGamePlayed(float betAmount, string pet, string city)
    {
        
    }

    public void GameCompleted(string pet, int distanceCovered, float winAmount)
    {
        
    }

    

    public void ChangeBalance(float amount, Action onAdd = null, string notice = "")
    {
        Debug.Log(amount);
        UpdateDB("balance", (balance + amount).ToString(), notice, () =>
        {
            balance += amount;
            onAdd?.Invoke();
        });
    }

    public void DeductBalance(float amount, Action onDeduct = null, string notice = "")
    {
        if (balance < amount) { NoticeUtils.ins.ShowOneBtnAlert("You have insufficient coins"); return; }
        UpdateDB("balance", (balance - amount).ToString(), notice, () =>
        {
            balance -= amount;
            onDeduct?.Invoke();
        });
    }

   
    public void UpdateDB(Dictionary<string, string> data, string notice = "", Action onDataSave = null, Action onError = null, string key = "", string val = "")
    {
        if (string.IsNullOrEmpty(playfabId)) { onDataSave?.Invoke(); return; }

        if (!string.IsNullOrEmpty(notice)) { NoticeUtils.ins.ShowLoadingAlert(notice); }
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest() { Data = data },
        result =>
        {
            if (!string.IsNullOrEmpty(notice)) { NoticeUtils.ins.HideLoadingAlert(); }
            onDataSave?.Invoke();

            if (string.IsNullOrEmpty(key)) 
            { Debug.Log("Successfully updated user data"); }
            else 
            { Debug.Log("Successfully updated user data: " + key + " = " + val); }
        },
        error =>
        {
            if (!string.IsNullOrEmpty(notice)) { NoticeUtils.ins.HideLoadingAlert(); }
            onError?.Invoke();
            Debug.Log("Error occurred saving additional data");
            Debug.Log(error.GenerateErrorReport());
        });
    }

    public void UpdateDB(string key, object val, string notice = "", Action onDataSave = null, Action onError = null) 
    { UpdateDB(key, JsonConvert.SerializeObject(val), notice, onDataSave, onError); }
    
    public void UpdateDB(string key, string val, string notice = "", Action onDataSave = null, Action onError = null) 
    { UpdateDB(new Dictionary<string, string>() { { key, val } }, notice, onDataSave, onError, key, val); }

    public void UpdateStatistic(string name, int val)
    {
        UpdatePlayerStatisticsRequest req = new UpdatePlayerStatisticsRequest() { Statistics = new List<StatisticUpdate>() { new StatisticUpdate() 
        { 
            StatisticName = name, Value = val
        }}};
        PlayFabClientAPI.UpdatePlayerStatistics(req,null,null);
    }


    void UpdateCurrencyUI(float amount, string currency, bool showNotification = false)
    {
        if (showNotification)
        {
            NoticeUtils.ins.ShowOneBtnAlert("Congratulations you have recieved " + amount + " " + currency);
        }
    }

    void GetServerTime()
    {
        PlayFabClientAPI.GetTime(new GetTimeRequest(), (res) =>
        { StartCoroutine("Timer", res.Time); },
        (err) =>
        {
            Debug.Log(err.ErrorDetails);
            GetServerTime();
        });
    }

    IEnumerator Timer(DateTime startTime)
    {
        int joinDuration = 0;
        while (true)
        {
            yield return new WaitForSeconds(1);
            joinDuration += 1;
            currentTime = startTime.AddSeconds(joinDuration);
            currentTimeString = currentTime.ToString();
        }
    }
    
}
