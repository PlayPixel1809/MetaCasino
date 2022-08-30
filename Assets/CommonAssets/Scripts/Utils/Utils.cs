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
        action?.Invoke(); 
    }

    public static void FrameDelayedAction(Action action, int frames = 1)
    {
        GameUtils.ins.StartCoroutine(FrameDelayedActionCoroutine(action, frames));
    }

    static IEnumerator FrameDelayedActionCoroutine(Action action, int frames = 1)
    {
        while (frames > 0)
        {
            frames -= 1;
            yield return new WaitForEndOfFrame();
        }
        action?.Invoke(); 
    }
}
