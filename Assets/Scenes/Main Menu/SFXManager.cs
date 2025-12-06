using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class SFXManager : MonoBehaviour
{
    // Instantiating the class as a singleton, only one at a time.
    public static SFXManager instance;

    [Header("Settings Panel Buttons")]
    public AudioSource SFXObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

    }

    public void PlaySFXClip(AudioClip audioClip, Transform spawnTransform, float volume, float pitch = 1f)
    {
        AudioSource audioSource = Instantiate(SFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.Play();
        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }

}
