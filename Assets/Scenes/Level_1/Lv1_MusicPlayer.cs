using UnityEngine;


public class Lv1_MusicPlayer : MonoBehaviour
{
    public AudioClip Level1_Theme;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MusicManager.instance.PlayMusicClip(Level1_Theme, transform, 0.5f, true);
    }
}
