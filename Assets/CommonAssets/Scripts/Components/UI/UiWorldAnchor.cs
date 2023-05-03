using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiWorldAnchor : MonoBehaviour
{
    public Transform anchorPoint;

    public float visibleDistance = 10;

    private Vector3 recScale;


    void Start()
    {
        recScale = transform.localScale;
        GameUtils.ins.StartCoroutine(InfrontOfCameraCheck()); 
    }

    void Update()
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(anchorPoint.transform.position); // pass the world position
        transform.position = screenPosition; // set the UI Transform's position as it will accordingly adjust the RectTransform values
    }

    IEnumerator InfrontOfCameraCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(.5f);

            if (anchorPoint == null) { yield break; }

            if (Vector3.Distance(anchorPoint.position, Camera.main.transform.position) > visibleDistance)
            {
                transform.localScale = Vector3.zero; // scale has been zeroed to just have an deactivate effect, gameObject.SetActive(false) stops all the coroutines running on any of the childrens
                continue;
            }

            Transform helper = null;
            if (Camera.main.transform.childCount == 0)
            {
                helper = new GameObject("UiWorldAnchorHelper").transform;
                helper.parent = Camera.main.transform;
                helper.localPosition = Vector3.zero;
            }

            helper = Camera.main.transform.GetChild(0);
            helper.LookAt(anchorPoint);

            if ((helper.localEulerAngles.y > 0 && helper.localEulerAngles.y < 60) || (helper.localEulerAngles.y > 300 && helper.localEulerAngles.y < 360))
            { transform.localScale = recScale; } 
            else 
            { transform.localScale = Vector3.zero; }
        }
    }
}
