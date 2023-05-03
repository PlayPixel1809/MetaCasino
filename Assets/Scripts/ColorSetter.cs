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

        Color color = new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f));
        material.color = color;
        //material.SetColor("_EmissionColor", color);

        if (grayScale)
        {
            float val = Random.Range(.25f, 1);
            material.color = new Color(val, val, val, 1);
            //material.SetColor("_EmissionColor", new Color(val, val, val, 1));
        }
    }
}
