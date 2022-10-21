using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Image timer;
    
    public void StartTimer(float time, Action onTimerComplete)
    {
        gameObject.SetActive(true);
        StartCoroutine(TimerCoroutine(time, onTimerComplete)); 
    }
    
    IEnumerator TimerCoroutine(float time, Action onTimerComplete)
    {
        float val = 0;
        while (val < 1)
        {
            val += Time.deltaTime/time;
            timer.fillAmount = val;
            yield return null;
        }
        onTimerComplete?.Invoke();
        gameObject.SetActive(false);
    }
}
