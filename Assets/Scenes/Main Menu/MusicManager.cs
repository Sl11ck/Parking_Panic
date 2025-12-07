using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    private List<AudioSource> activeMusicSources = new List<AudioSource>();
    private AudioSource _activeLevelMusicSource; 
    private AudioSource _activePauseMusicSource;

    public static MusicManager instance;

    [Header("Settings")]
    public AudioSource MusicObject; 
    public AudioMixerGroup musicMixerGroup; 

    private void Awake()
    {
        // 1. NUCLEAR SINGLETON: Kill the old manager from Main Menu
        // This ensures Level 1 gets a fresh manager with correct references
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // 2. SAFETY: Ensure the global Audio Listener is ON
        AudioListener.pause = false; 
    }

    public void PlayMusicClip(AudioClip audioClip, Transform spawnTransform, float volume, bool loop)
    {
        // Instantiate the audio source
        AudioSource audioSource = Instantiate(MusicObject, spawnTransform.position, Quaternion.identity);

        // FIX: Force connection to Mixer
        if (musicMixerGroup != null) audioSource.outputAudioMixerGroup = musicMixerGroup;

        // FIX: Force sound to be 2D (Spatial Blend = 0)
        // This ensures the volume is the same no matter where the camera is
        audioSource.spatialBlend = 0f; 

        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.Play();

        activeMusicSources.Add(audioSource);
        _activeLevelMusicSource = audioSource; 

        if (!loop)
        {
            Destroy(audioSource.gameObject, audioSource.clip.length);
        }
    }

    public void SwapToPauseMusic(AudioClip pauseClip, float volume)
    {
        if (_activeLevelMusicSource != null) _activeLevelMusicSource.Pause();

        _activePauseMusicSource = Instantiate(MusicObject, transform.position, Quaternion.identity);

        if (musicMixerGroup != null) _activePauseMusicSource.outputAudioMixerGroup = musicMixerGroup;
        
        // FIX: Force 2D here too
        _activePauseMusicSource.spatialBlend = 0f;

        _activePauseMusicSource.clip = pauseClip;
        _activePauseMusicSource.volume = volume;
        _activePauseMusicSource.loop = true;
        _activePauseMusicSource.ignoreListenerPause = true; 
        _activePauseMusicSource.Play();

        activeMusicSources.Add(_activePauseMusicSource);
    }

    public void ResumeLevelMusic()
    {
        if (_activePauseMusicSource != null)
        {
            _activePauseMusicSource.Stop();
            activeMusicSources.Remove(_activePauseMusicSource);
            Destroy(_activePauseMusicSource.gameObject);
            _activePauseMusicSource = null;
        }

        if (_activeLevelMusicSource != null)
        {
            _activeLevelMusicSource.UnPause();
        }
    }

    public void StopAllMusic()
    {
        for (int i = activeMusicSources.Count - 1; i >= 0; i--)
        {
            if (activeMusicSources[i] != null)
            {
                activeMusicSources[i].Stop();
                Destroy(activeMusicSources[i].gameObject);
            }
        }
        activeMusicSources.Clear();
        _activeLevelMusicSource = null;
        _activePauseMusicSource = null;
    }
}