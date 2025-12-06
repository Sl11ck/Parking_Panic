using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private List<AudioSource> activeMusicSources = new List<AudioSource>();
    
    // References to track specific audio sources so we can control them individually
    private AudioSource _activeLevelMusicSource; 
    private AudioSource _activePauseMusicSource;

    public static MusicManager instance;

    [Header("Settings Panel Buttons")]
    public AudioSource MusicObject; // The Prefab used to spawn audio

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keeps music playing between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusicClip(AudioClip audioClip, Transform spawnTransform, float volume, bool loop)
    {
        AudioSource audioSource = Instantiate(MusicObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.Play();

        // Add to tracking list
        activeMusicSources.Add(audioSource);

        // --- ADDITION 1: Track this as the current level music ---
        // We assume the last clip played via this method is the main level theme
        _activeLevelMusicSource = audioSource; 

        if (!loop)
        {
            float clipLength = audioSource.clip.length;
            Destroy(audioSource.gameObject, clipLength);
        }
    }

    // --- ADDITION 2: New Methods for Pause Menu Logic ---

    public void SwapToPauseMusic(AudioClip pauseClip, float volume)
    {
        // 1. Pause the Level Music (This automatically saves the time/position)
        if (_activeLevelMusicSource != null)
        {
            _activeLevelMusicSource.Pause();
        }

        // 2. Instantiate the Pause Music
        // We spawn a new object specifically for the pause loop
        _activePauseMusicSource = Instantiate(MusicObject, transform.position, Quaternion.identity);
        _activePauseMusicSource.clip = pauseClip;
        _activePauseMusicSource.volume = volume;
        _activePauseMusicSource.loop = true;
        
        // Ensure pause music plays even if Time.timeScale is 0
        _activePauseMusicSource.ignoreListenerPause = true; 
        _activePauseMusicSource.Play();

        activeMusicSources.Add(_activePauseMusicSource);
    }

    public void ResumeLevelMusic()
    {
        // 1. Destroy the Pause Music
        if (_activePauseMusicSource != null)
        {
            _activePauseMusicSource.Stop();
            activeMusicSources.Remove(_activePauseMusicSource); // Clean up list
            Destroy(_activePauseMusicSource.gameObject);
            _activePauseMusicSource = null;
        }

        // 2. UnPause the Level Music (Resumes from saved time)
        if (_activeLevelMusicSource != null)
        {
            _activeLevelMusicSource.UnPause();
        }
    }

    // ----------------------------------------------------

    public void StopAllMusic()
    {
        // Stop and destroy all tracked music sources
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