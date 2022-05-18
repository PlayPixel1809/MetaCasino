using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDeactivate : MonoBehaviour
{
    public float deactivateTime;
    
    void OnEnable()
    {
        StartCoroutine("SelfDestroyCoroutine");
    }

    IEnumerator SelfDestroyCoroutine()
    {
        yield return new WaitForSeconds(deactivateTime);
        gameObject.SetActive(false);
    }
}
