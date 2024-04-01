using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class JsMethods : MonoBehaviour
{
    public static JsMethods ins;
    void Awake() { ins = this; }

    [DllImport("__Internal")]
    public static extern void GetToken();


    public Text text;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.T))
        {
            GetToken();
        }
    }
}

