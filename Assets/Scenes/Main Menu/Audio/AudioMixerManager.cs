using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerManager : MonoBehaviour
{
    [SerializeField] public AudioMixer audioMixer;

    public void SetMasterVolume(float level)
    {
        // audioMixer.SetFloat("masterVolume", level);
        audioMixer.SetFloat("masterVolume", Mathf.Log10(level) * 20f);
    }

    public void SetSFXVolume(float level)
    {
        // audioMixer.SetFloat("soundVolume", level);
        audioMixer.SetFloat("soundVolume", Mathf.Log10(level) * 20f);
    }

    public void SetMusicVolume(float level)
    {

        // audioMixer.SetFloat("musicVolume", level);
        audioMixer.SetFloat("musicVolume", Mathf.Log10(level) * 20f);
    }
}
