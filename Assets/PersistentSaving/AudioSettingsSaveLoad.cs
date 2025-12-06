using UnityEngine;
using UnityEngine.Audio;
using System.IO;
using System;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class AudioSettingsSaveLoad : MonoBehaviour
{
    [SerializeField] private AudioMixerManager audioMixerManager;
    private const float DEFAULT_VOLUME = 1f;
    private string audioSavePath;

    public Slider MasterVolSlider;
    public Slider SFXVolSlider;
    public Slider MusicVolSlider;
    public TextMeshProUGUI MasterVolText;
    public TextMeshProUGUI SFXVolText;
    public TextMeshProUGUI MusicVolText;

    private void Start()
    {
        MasterVolSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        SFXVolSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        MusicVolSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
    }

    private void Awake()
    {
        // Set the save path relative to the current folder
        string currentDirectory = Path.GetDirectoryName(Application.dataPath);
        audioSavePath = Path.Combine(currentDirectory, "Assets/PersistentSaving/jsonSaveData", "AudioSettingsData.json");
        Debug.Log(audioSavePath);

        // Ensure the directory exists
        string directory = Path.GetDirectoryName(audioSavePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    // Updating the volume percentage text on slider movement.
// Updating the volume percentage text AND the actual AudioMixer
    private void OnMasterVolumeChanged(float value)
    {
        // Update the visual text
        MasterVolText.text = $"{Mathf.RoundToInt(value * 100f)}%";
        
        // FIX: Actually send the new value to the mixer
        if(audioMixerManager != null) 
            audioMixerManager.SetMasterVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        SFXVolText.text = $"{Mathf.RoundToInt(value * 100f)}%";

        // FIX: Actually send the new value to the mixer
        if(audioMixerManager != null)
            audioMixerManager.SetSFXVolume(value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        MusicVolText.text = $"{Mathf.RoundToInt(value * 100f)}%";

        // FIX: Actually send the new value to the mixer
        if(audioMixerManager != null)
            audioMixerManager.SetMusicVolume(value);
    }  

    // Save audio settings to JSON
    public void SaveAudioSettings()
    {
        // Get current volume levels (convert from dB to linear)
        float masterLinear = GetLinearVolume("masterVolume");
        float sfxLinear = GetLinearVolume("soundVolume");
        float musicLinear = GetLinearVolume("musicVolume");

        AudioSettingsData data = new AudioSettingsData(masterLinear, sfxLinear, musicLinear);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(audioSavePath, json);
        
        Debug.Log($"Audio settings saved to: {audioSavePath}");
    }

    // Load audio settings from JSON
    public void LoadAudioSettings()
    {
        if (File.Exists(audioSavePath))
        {
            string json = File.ReadAllText(audioSavePath);
            AudioSettingsData data = JsonUtility.FromJson<AudioSettingsData>(json);

            // Apply loaded settings using the audioMixerManager instance
            audioMixerManager.SetMasterVolume(data.masterVolume);
            audioMixerManager.SetSFXVolume(data.sfxVolume);
            audioMixerManager.SetMusicVolume(data.musicVolume);

            MasterVolSlider.value = data.masterVolume;
            SFXVolSlider.value = data.sfxVolume;
            MusicVolSlider.value = data.musicVolume;

            MasterVolText.text = $"{Mathf.RoundToInt(data.masterVolume * 100f)}%";
            SFXVolText.text = $"{Mathf.RoundToInt(data.sfxVolume * 100f)}%";
            MusicVolText.text = $"{Mathf.RoundToInt(data.musicVolume * 100f)}%";
            
            Debug.Log("Audio settings loaded successfully!");
        }
        else
        {
            // Set default values if no save file exists
            audioMixerManager.SetMasterVolume(DEFAULT_VOLUME);
            audioMixerManager.SetSFXVolume(DEFAULT_VOLUME);
            audioMixerManager.SetMusicVolume(DEFAULT_VOLUME);
            MasterVolText.text = $"{Mathf.RoundToInt(DEFAULT_VOLUME * 100f)}%";
            SFXVolText.text = $"{Mathf.RoundToInt(DEFAULT_VOLUME * 100f)}%";
            MusicVolText.text = $"{Mathf.RoundToInt(DEFAULT_VOLUME * 100f)}%";

            Debug.Log("No saved audio settings found. Using defaults.");
        }
    }
    
    // Helper method to convert dB to linear volume
    private float GetLinearVolume(string volumeParameter)
    {
        if (audioMixerManager.audioMixer.GetFloat(volumeParameter, out float dB))
        {
            // If volume is at minimum (-80dB), return 0
            if (dB <= -80f) return 0f;
            
            // Convert dB to linear (0-1)
            return Mathf.Pow(10f, dB / 20f);
        }
        return DEFAULT_VOLUME;
    }
}