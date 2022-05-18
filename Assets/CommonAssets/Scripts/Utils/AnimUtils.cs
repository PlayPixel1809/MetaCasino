using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimUtils 
{
    public static void Transform(Transform transform, Vector3 from, Vector3 to, float time, Space space, Action onAnimationComplete = null)
    {
        GameUtils.ins.StartCoroutine(AnimateTransformCoroutine(transform, from, to, time, AnimationCurve.Linear(0,0,1,1), space, onAnimationComplete));
    }

    public static void Transform(Transform transform, Vector3 from, Vector3 to, float time, Space space, AnimationCurve animationCurve, Action onAnimationComplete = null)
    {
        GameUtils.ins.StartCoroutine(AnimateTransformCoroutine(transform, from, to, time, animationCurve, space, onAnimationComplete));
    }

    static IEnumerator AnimateTransformCoroutine(Transform transform, Vector3 from, Vector3 to, float time, AnimationCurve animationCurve, Space space, Action onAnimationComplete = null)
    {
        float val = 0;
        while (val < 1)
        {
            val += Time.deltaTime / time;
            if (space == Space.Self)  { transform.localPosition = Vector3.Lerp(from, to, animationCurve.Evaluate(val)); }
            if (space == Space.World) { transform.position = Vector3.Lerp(from, to, animationCurve.Evaluate(val)); }
            yield return null;
        }
        onAnimationComplete?.Invoke();
    }

    public static void FadeUi(Transform ui, float from, float to, float time, Action onAnimationComplete = null)
    {
        GameUtils.ins.StartCoroutine(FadeUiCoroutine(ui.GetComponent<Image>(), ui.GetComponent<Text>(), from, to, time, onAnimationComplete));
    }

    static IEnumerator FadeUiCoroutine(Image img, Text txt, float from, float to, float time, Action onAnimationComplete = null)
    {
        float val = 0;
        while (val < 1)
        {
            val += Time.deltaTime / time;
            if (img != null) { img.color = new Color(img.color.r, img.color.g, img.color.b, Mathf.Lerp(from, to, val)); }
            if (txt != null) { txt.color = new Color(img.color.r, img.color.g, img.color.b, Mathf.Lerp(from, to, val)); }
            yield return null;
        }
        onAnimationComplete?.Invoke();
    }

    public static void UiColor(Transform ui, Color from, Color to, float time, Action onAnimationComplete = null)
    {
        GameUtils.ins.StartCoroutine(UiColorCoroutine(ui.GetComponent<Image>(), ui.GetComponent<Text>(), from, to, time, onAnimationComplete));
    }

    static IEnumerator UiColorCoroutine(Image img, Text txt, Color from, Color to, float time, Action onAnimationComplete = null)
    {
        float val = 0;
        while (val < 1)
        {
            val += Time.deltaTime / time;
            if (img != null) { img.color = Color.Lerp(from, to, val); }
            if (txt != null) { txt.color = Color.Lerp(from, to, val); }
            yield return null;
        }
        onAnimationComplete?.Invoke();
    }
}
