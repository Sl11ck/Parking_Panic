using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Pause_Menu : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject pausePanel;
    public Button ResumeBtn;
    public Button RetryBtn;
    public Button QuitBtn;


    [Header("Audio Settings")]
    public AudioClip pauseMusicClip;
    [Range(0f, 1f)] public float pauseMusicVolume = 0.5f;

    [SerializeField] private AudioSettingsSaveLoad audioSettingsSaveLoad;

    void Start()
    {
        pausePanel.SetActive(false);
        ResumeBtn.onClick.AddListener(ResumeGameplay);
        RetryBtn.onClick.AddListener(RestartGameplay);
        QuitBtn.onClick.AddListener(LoadMainMenu);
        audioSettingsSaveLoad.LoadAudioSettings();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausePanel.activeSelf)
                ResumeGameplay();
            else
                PauseGameplay();
        }
    }

    public void PauseGameplay()
    {
        pausePanel.SetActive(true);
        
        // FIX: Switch music when pausing
        if (MusicManager.instance != null && pauseMusicClip != null)
        {
            MusicManager.instance.SwapToPauseMusic(pauseMusicClip, pauseMusicVolume);
        }

        Time.timeScale = 0f;
    }

    public void ResumeGameplay()
    {
        Time.timeScale = 1f;
        audioSettingsSaveLoad.SaveAudioSettings();
        pausePanel.SetActive(false);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // FIX: Restore the original music
        if (MusicManager.instance != null)
        {
            MusicManager.instance.ResumeLevelMusic();
        }
    }

    public static string SceneToLoad;
    public void RestartGameplay()
    {
        audioSettingsSaveLoad.SaveAudioSettings();
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        audioSettingsSaveLoad.SaveAudioSettings();
        Time.timeScale = 1f;
        audioSettingsSaveLoad.SaveAudioSettings();
        SceneManager.LoadScene("Main_Menu");
    }
}
