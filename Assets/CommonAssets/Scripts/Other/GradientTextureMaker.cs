// returns gradient Texture2D (size=256x1)
using UnityEngine;


public class GradientTextureMaker : MonoBehaviour
{
    public enum GradientTypes { Horizontal, Vertical}

    public GradientTypes gradientType;
    
    public Material material;
    public Color[] colors;

    int width = 0;
    int height = 0;

    void Start()
    {
        if (gradientType == GradientTypes.Horizontal)
        {
            width = 256;
            height = 1;
        }
        else
        {
            width = 1;
            height = 256;
        }

        // generate texture and assign as main texture
        CreateGradientTexture();
    }

    public void CreateGradientTexture()
    {
        material.SetTexture("_MainTex", Create(colors));
        material.SetTexture("_EmissionMap", Create(colors));
    } 


    public Texture2D Create(Color[] colors, TextureWrapMode textureWrapMode = TextureWrapMode.Clamp, FilterMode filterMode = FilterMode.Point, bool isLinear = false, bool hasMipMap = false)
    {
        if (colors == null || colors.Length == 0)
        {
            Debug.LogError("No colors assigned");
            return null;
        }

        int length = colors.Length;
        if (colors.Length > 8)
        {
            Debug.LogWarning("Too many colors! maximum is 8, assigned: " + colors.Length);
            length = 8;
        }

        // build gradient from colors
        var colorKeys = new GradientColorKey[length];
        var alphaKeys = new GradientAlphaKey[length];

        float steps = length - 1f;
        for (int i = 0; i < length; i++)
        {
            float step = i / steps;
            colorKeys[i].color = colors[i];
            colorKeys[i].time = step;
            alphaKeys[i].alpha = colors[i].a;
            alphaKeys[i].time = step;
        }

        // create gradient
        Gradient gradient = new Gradient();
        gradient.SetKeys(colorKeys, alphaKeys);

        // create texture
        Texture2D outputTex = new Texture2D(width, height, TextureFormat.ARGB32, false, isLinear);
        outputTex.wrapMode = textureWrapMode;
        outputTex.filterMode = filterMode;

        // draw texture

        if (gradientType == GradientTypes.Horizontal)
        {
            for (int i = 0; i < width; i++)
            {
                outputTex.SetPixel(i, 0, gradient.Evaluate((float)i / (float)width));
            }
        }
        else
        {
            for (int i = 0; i < height; i++)
            {
                outputTex.SetPixel(0, i, gradient.Evaluate((float)i / (float)height));
            }
        }
        
        outputTex.Apply(false);

        return outputTex;
    } // BuildGradientTexture

} // class
