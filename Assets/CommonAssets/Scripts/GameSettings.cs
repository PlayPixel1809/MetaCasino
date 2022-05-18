using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    public Slider musicVolSlider;
    public Slider effectsVolSlider;


    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        if (!PlayerPrefs.HasKey("musicVol"))   { PlayerPrefs.SetFloat("musicVol", .5f); }
        if (!PlayerPrefs.HasKey("effectsVol")) { PlayerPrefs.SetFloat("effectsVol", .5f); }

        if (musicVolSlider   != null) { musicVolSlider.value = PlayerPrefs.GetFloat("musicVol");     }
        if (effectsVolSlider != null) { effectsVolSlider.value = PlayerPrefs.GetFloat("effectsVol"); }

    }

    public void MusicVolSlider(Slider slider)
    {
        PlayerPrefs.SetFloat("musicVol", slider.value);
        if (MusicUtils.ins != null) { MusicUtils.ins.SetVolume(slider.value); }
    }

    

    public void EffectsVolSlider(Slider slider)
    {
        PlayerPrefs.SetFloat("effectsVol", slider.value);
    }
}
