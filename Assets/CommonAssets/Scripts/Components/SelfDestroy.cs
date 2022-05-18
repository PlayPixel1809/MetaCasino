using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    public float destroyTime;
    
    void OnEnable()
    {
        StartCoroutine("SelfDestroyCoroutine");
    }

    IEnumerator SelfDestroyCoroutine()
    {
        yield return new WaitForSeconds(destroyTime);
        Destroy(gameObject);
    }
}
