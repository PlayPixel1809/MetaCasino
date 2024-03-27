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
    private static extern void Hello();

    [DllImport("__Internal")]
    private static extern void HelloString(string str);

    [DllImport("__Internal")]
    private static extern void PrintFloatArray(float[] array, int size);

    [DllImport("__Internal")]
    private static extern int AddNumbers(int x, int y);

    [DllImport("__Internal")]
    private static extern string StringReturnValueFunction();

    [DllImport("__Internal")]
    public static extern string GetEmailFromUrl();

    [DllImport("__Internal")]
    public static extern string GetPasswordFromUrl();

    [DllImport("__Internal")]
    public static extern void GetToken();

    [DllImport("__Internal")]
    private static extern void BindWebGLTexture(int texture);

    

    public Text text;

    void Start()
    {
        //text.text = GetDataFromUrl("email");

        //HelloString("This is a string.");

        //float[] myArray = new float[10];
        //PrintFloatArray(myArray, myArray.Length);

        //int result = AddNumbers(5, 7);
        //text.text = result.ToString();
        //Debug.Log(result);

        //Debug.Log(StringReturnValueFunction());

        //var texture = new Texture2D(0, 0, TextureFormat.ARGB32, false);
        //BindWebGLTexture(texture.GetNativeTexturePtr());
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.T))
        {
            GetToken();
        }

        if (Input.GetKeyUp(KeyCode.E)) 
        {
            text.text = GetEmailFromUrl();
        }

        if (Input.GetKeyUp(KeyCode.P))
        {
            text.text = GetPasswordFromUrl();
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            text.text = AddNumbers(10,10).ToString();
        }

        if (Input.GetKeyUp(KeyCode.C))
        {
            text.text = StringReturnValueFunction();
        }
    }
}

