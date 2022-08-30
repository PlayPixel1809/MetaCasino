using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSetter : MonoBehaviour
{
    public bool grayScale;
    public Material material;
    
    void Start()
    {
        if (material == null) { material = GetComponent<Renderer>().material; }
        
        material.color = new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f),1);

        if (grayScale)
        {
            float val = Random.Range(.25f, 1);
            material.color = new Color(val, val, val, 1);
        }
    }
}
