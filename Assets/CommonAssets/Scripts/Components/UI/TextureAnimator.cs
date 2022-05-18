using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureAnimator : MonoBehaviour
{
    public Renderer renderer;

    public string texPropertyName = "_MainTex";

    public bool playOnEnable = true;
    public float animTime = 1;
    public bool loop = true;
    public List<Sprite> sprites;


    void OnEnable()
    {
        if (playOnEnable && sprites.Count > 0) { StartCoroutine("SpriteAnimation"); }
    }

    IEnumerator SpriteAnimation()
    {
        while (true)
        {
            for (int i = 0; i < sprites.Count; i++)
            {
                renderer.material.SetTexture(texPropertyName, sprites[i].texture);
                yield return new WaitForSeconds(animTime / sprites.Count);
            }
            if (!loop) { yield break; }
        }
    }
}
