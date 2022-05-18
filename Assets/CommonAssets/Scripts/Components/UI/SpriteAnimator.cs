using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteAnimator : MonoBehaviour
{
    public Image image;
    public SpriteRenderer spriteRenderer;

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
                if (image != null) { image.sprite = sprites[i]; }
                if (spriteRenderer != null) { spriteRenderer.sprite = sprites[i]; }
                yield return new WaitForSeconds(animTime / sprites.Count);
            }
            if (!loop) { yield break; }
        }
    }
}
