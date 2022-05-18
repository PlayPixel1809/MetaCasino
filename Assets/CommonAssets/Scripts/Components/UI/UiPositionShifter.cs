using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiPositionShifter : MonoBehaviour
{
    public Vector2 lowResolution = new Vector2(1920, 1080);
    public Vector2 lowResolutionPos = new Vector2(1, 1);

    public Vector2 highResolution = new Vector2(2340, 1080);
    public Vector2 highResolutionPos = new Vector2(1, 1);



    void Start()
    {
        SetScale();
    }

    //This method will calculate and apply the scale on the self transform based on the defined properties 
    void SetScale()
    {
        float lowResAspect = lowResolution.y / lowResolution.x;
        float highResAspect = highResolution.y / highResolution.x;

        if (Screen.width > Screen.height)
        {
            lowResAspect = lowResolution.x / lowResolution.y;
        }
        highResAspect = highResolution.x / highResolution.y;

        float aspectWindow = highResAspect - lowResAspect;
        float screenAspect = Mathf.Clamp((float)Screen.height / Screen.width, lowResAspect, highResAspect);

        if (Screen.width > Screen.height)
        {
            screenAspect = Mathf.Clamp((float)Screen.width / Screen.height, lowResAspect, highResAspect);
        }

        float finalAspect = screenAspect - lowResAspect;

        Vector2 finalPos = Vector2.Lerp(lowResolutionPos, highResolutionPos, finalAspect / aspectWindow);
        transform.localPosition = new Vector3(finalPos.x, finalPos.y, 0);
    }
}
