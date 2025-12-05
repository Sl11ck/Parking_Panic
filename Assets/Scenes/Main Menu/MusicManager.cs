using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class MusicManager : MonoBehaviour
{
    private List<AudioSource> activeMusicSources = new List<AudioSource>();
    // Instantiating the class as a singleton, only one at a time.
    public static MusicManager instance;

    [Header("Settings Panel Buttons")]
    public AudioSource MusicObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

    }

    public void PlayMusicClip(AudioClip audioClip, Transform spawnTransform, float volume, bool loop)
    {
        AudioSource audioSource = Instantiate(MusicObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.Play();

        // Add to tracking list, for deleting all music when called.
        activeMusicSources.Add(audioSource);

        if (!loop)
        {
            float clipLength = audioSource.clip.length;
            Destroy(audioSource.gameObject, clipLength);
        }
    }

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
    }    

}