using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoroutineUtils
{
    public static List<AdvanceCoroutine> advanceCoroutines = new List<AdvanceCoroutine>();
    
    public class AdvanceCoroutine
    {
        public Coroutine wrapperCoroutine;

        public MonoBehaviour mono;
        public Coroutine coroutine;
        public IEnumerator enumerator;
        public Action onCoroutineComplete;

        public void Invoke()
        {
            wrapperCoroutine = mono.StartCoroutine(InvokeCoroutine());
        }

        IEnumerator InvokeCoroutine()
        {
            if (enumerator != null)
            {
                coroutine = mono.StartCoroutine(enumerator);
                yield return coroutine;
            }
            if (onCoroutineComplete != null) { onCoroutineComplete.Invoke(); }
            advanceCoroutines.Remove(this);
        }

        public void Finish()
        {
            if (mono != null)
            {
                mono.StopCoroutine(coroutine);
                mono.StopCoroutine(wrapperCoroutine);
                if (onCoroutineComplete != null) { onCoroutineComplete.Invoke(); }
            }
            advanceCoroutines.Remove(this);
        }
    }
    
    public static void StartCoroutine(MonoBehaviour mono, IEnumerator enumerator, Action onCoroutineComplete)
    {
        AdvanceCoroutine advanceCoroutine = new AdvanceCoroutine();
        advanceCoroutines.Add(advanceCoroutine);
        advanceCoroutine.mono = mono;
        advanceCoroutine.enumerator = enumerator;
        advanceCoroutine.onCoroutineComplete = onCoroutineComplete;
        advanceCoroutine.Invoke();
        
    }

    public static void StartCoroutineSolo(MonoBehaviour mono, IEnumerator enumerator, Action onCoroutineComplete)
    {
        FinishAll();
        StartCoroutine(mono, enumerator, onCoroutineComplete);
    }

    public static void FinishAll()
    {
        for (int i = 0; i < advanceCoroutines.Count; i++)
        {
            advanceCoroutines[i].Finish();
        }
    }
}
