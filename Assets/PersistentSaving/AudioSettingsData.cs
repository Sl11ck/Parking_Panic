using UnityEngine;

[System.Serializable]
public class AudioSettingsData
{
    public float masterVolume;
    public float sfxVolume;
    public float musicVolume;
    
    public AudioSettingsData(float master, float sfx, float music)
    {
        masterVolume = master;
        sfxVolume = sfx;
        musicVolume = music;
    }
}
