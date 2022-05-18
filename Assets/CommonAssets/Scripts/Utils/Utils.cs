using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils 
{
    public static void InvokeDelayedAction(float delay, Action action)
    {
        GameUtils.ins.StartCoroutine(DelayedActionCoroutine(delay, action));
    }

    static IEnumerator DelayedActionCoroutine(float delay, Action action)
    {
        if (delay == 0) { yield return new WaitForEndOfFrame(); }
        if (delay > 0)  { yield return new WaitForSeconds(delay); }
        if (action != null) { action.Invoke(); }
    }
}
