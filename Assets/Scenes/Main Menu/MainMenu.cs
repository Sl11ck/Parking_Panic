using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using TMPro;
public class PanelManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject startPanel;
    public GameObject mainPanel;
    public GameObject playPanel;
    public GameObject settingsPanel;
    public GameObject videoSettingsPanel;
    public GameObject audioSettingsPanel;
    public GameObject creditsPanel;

    [Header("Start Panel Element")]
    public TextMeshProUGUI StartScreenText;
    public float startScreenTxtFadeDuration = 1.5f; 

    [Header("Main Panel Buttons")]
    public Button playBtn;
    public Button settingsBtn;
    public Button creditsBtn;
    public Button exitBtn;

    [Header("Play Panel Buttons")]
    public Button Lvl1ModeBtn;
    public Button Lvl2ModeBtn;
    public Button Lvl3ModeBtn;
    public Button Lvl4ModeBtn;
    public Button Lvl5ModeBtn;
    public Button pbackBtn;

    [Header("Settings Panel Buttons")]
    public Button VideoBtn;
    public Button SaveVideoChangesBtn;
    public Button AudioBtn;
    // The SaveAudioChangesBtn is the sbackBtn.
    public Button sbackBtn;

    [Header("Credits Panel Settings")]
    public float scrollDuration = 5f;
    public float scrollDistance = 1.5f;
    public float fadeDuration = 1f;

    [Header("Menu SFX")]
    public AudioClip acceptSFX;
    public AudioClip declineSFX;
    public AudioClip highlightSFX;
    public AudioClip MainMenuMusic;
    public AudioClip StartPanelMusic;
    [SerializeField] private AudioSettingsSaveLoad audioSettingsSaveLoad;
    [SerializeField] private VideoSettingsSaveLoad videoSettingsSaveLoad;

    private void Start()
    {
        // Initialize panels
        SetAllPanelsInactive();

        // Show the start screen, play it's music.
        startPanel.SetActive(true);
        StartCoroutine(FadeStartScreenText());
        PlayStartPanelMusic();

        // when a key is pressed hide the start screen, show the main menu.
        InputSystem.onAnyButtonPress.CallOnce(ctrl => StartToMainPanelnMusic());

        // Set up Main Panel button listeners
        playBtn.onClick.AddListener(ShowPlayPanel);
        settingsBtn.onClick.AddListener(ShowSettingsPanel);
        creditsBtn.onClick.AddListener(ShowCreditsPanel);
        exitBtn.onClick.AddListener(ExitApplication);
        SetupButtonHover(playBtn);
        SetupButtonHover(settingsBtn);
        SetupButtonHover(creditsBtn);
        SetupButtonHover(exitBtn);

        // Set up Play Panel button listeners
        Lvl1ModeBtn.onClick.AddListener(LoadLvl1Mode);
        Lvl2ModeBtn.onClick.AddListener(LoadLvl2Mode);
        Lvl3ModeBtn.onClick.AddListener(LoadLvl3Mode);
        Lvl4ModeBtn.onClick.AddListener(LoadLvl4Mode);
        Lvl5ModeBtn.onClick.AddListener(LoadLvl5Mode);

        pbackBtn.onClick.AddListener(PlayBackToMainPanel);
        SetupButtonHover(Lvl1ModeBtn);
        SetupButtonHover(Lvl2ModeBtn);
        SetupButtonHover(Lvl3ModeBtn);
        SetupButtonHover(Lvl4ModeBtn);
        SetupButtonHover(Lvl5ModeBtn);
        SetupButtonHover(pbackBtn);

        // Set up Settings Panel button listeners
        VideoBtn.onClick.AddListener(ShowVideoSettingsPanel);
        SaveVideoChangesBtn.onClick.AddListener(SaveVideoSettingsChanges);
        AudioBtn.onClick.AddListener(ShowAudioSettingsPanel);
        sbackBtn.onClick.AddListener(SettingsBackToMainPanel);
        SetupButtonHover(VideoBtn);
        SetupButtonHover(AudioBtn);
        SetupButtonHover(sbackBtn);

        // Initialize credits references
        creditsCanvasGroup = creditsPanel.GetComponent<CanvasGroup>();
        if (creditsCanvasGroup == null)
            creditsCanvasGroup = creditsPanel.AddComponent<CanvasGroup>();

        creditsRectTransform = creditsPanel.GetComponent<RectTransform>();
        creditsStartPosition = creditsRectTransform.anchoredPosition;

        // Load saved audio settings into Unity audio mixer
        audioSettingsSaveLoad.LoadAudioSettings();
        // Load saved video settings
        videoSettingsSaveLoad.AddExtraResolutionOptionToArray();
        videoSettingsSaveLoad.LoadVideoSettings();
        videoSettingsSaveLoad.ApplyVideoSettings();
    }

    // START PANEL              START
    private void StartToMainPanelnMusic()
    {
        startPanel.SetActive(false);
        mainPanel.SetActive(true);
        MusicManager.instance.StopAllMusic();
        PlayMainMenuMusic();
    }
    private IEnumerator FadeStartScreenText()
    {
        while (true)
        {
            yield return StartCoroutine(FadeStartScreenTextAlpha(0f, 1f, fadeDuration));
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(FadeStartScreenTextAlpha(1f, 0f, fadeDuration));
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    private IEnumerator FadeStartScreenTextAlpha(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            
            Color color = StartScreenText.color;
            color.a = currentAlpha;
            StartScreenText.color = color;
            
            yield return null;
        }
    }    
    // START PANEL              END

    private void SetAllPanelsInactive()
    {
        startPanel.SetActive(false);
        mainPanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        playPanel.SetActive(false);
        videoSettingsPanel.SetActive(false);
        audioSettingsPanel.SetActive(false);
    }

    public void ShowMainPanel()
    {
        SetAllPanelsInactive();
        mainPanel.SetActive(true);
    }

    // AUDIO MENU SFX Music      START
    protected void PlayAcceptSFX()
    {
        SFXManager.instance.PlaySFXClip(acceptSFX, transform, 1f);
    }
    protected void PlayDeclineSFX()
    {
        SFXManager.instance.PlaySFXClip(declineSFX, transform, 1f);
    }
    protected void PlayHighlightSFX()
    {
        SFXManager.instance.PlaySFXClip(highlightSFX, transform, 1f);
    }
    protected void PlayMainMenuMusic()
    {
        MusicManager.instance.PlayMusicClip(MainMenuMusic, transform, 0.5f, true);
    }
    protected void PlayStartPanelMusic()
    {
        MusicManager.instance.PlayMusicClip(StartPanelMusic, transform, 0.5f, true);
    }    
    private void SetupButtonHover(Button button)
    {
    EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
    EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry();
    pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
    pointerEnterEntry.callback.AddListener((data) => { PlayHighlightSFX(); });
    trigger.triggers.Add(pointerEnterEntry);
    }
    // AUDIO MENU SFX Music      END

    // PLAY CONTROL SECTION      START
    public void ShowPlayPanel()
    {
        PlayAcceptSFX();
        SetAllPanelsInactive();
        playPanel.SetActive(true);
    }

    public void LoadLvl1Mode()
    {
        StartCoroutine(LoadNextSceneAfterSFX("Level1"));
    }
    public void LoadLvl2Mode()
    {
        StartCoroutine(LoadNextSceneAfterSFX("Level2"));
    }
    public void LoadLvl3Mode()
    {
        StartCoroutine(LoadNextSceneAfterSFX("Level3"));
    }
    public void LoadLvl4Mode()
    {
        StartCoroutine(LoadNextSceneAfterSFX("Level4"));
    }
    public void LoadLvl5Mode()
    {
        StartCoroutine(LoadNextSceneAfterSFX("Level5"));
    }


    protected IEnumerator LoadNextSceneAfterSFX(string sceneName)
    {
        PlayAcceptSFX();
        yield return new WaitForSeconds(acceptSFX.length);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
    public void PlayBackToMainPanel()
    {
        PlayDeclineSFX();
        ShowMainPanel();
    }
    // PLAY CONTROL SECTION      END

    // SETTINGS CONTROL SECTION      START
    public void ShowSettingsPanel()
    {
        PlayAcceptSFX();
        SetAllPanelsInactive();
        settingsPanel.SetActive(true);
    }

    public void ShowVideoSettingsPanel()
    {
        PlayAcceptSFX();
        SetAllPanelsInactive();
        videoSettingsSaveLoad.LoadVideoSettings();
        settingsPanel.SetActive(true);
        videoSettingsPanel.SetActive(true);
    }
    public void SaveVideoSettingsChanges()
    {
        PlayAcceptSFX();
        SetAllPanelsInactive();
        audioSettingsSaveLoad.SaveAudioSettings();
        videoSettingsSaveLoad.SaveVideoSettings();
        ShowMainPanel();
    }

    public void ShowAudioSettingsPanel()
    {
        PlayAcceptSFX();
        SetAllPanelsInactive();
        settingsPanel.SetActive(true);
        audioSettingsPanel.SetActive(true);
        videoSettingsSaveLoad.ClearTemporarySettings();
    }

    public void SettingsBackToMainPanel()
    {
        PlayDeclineSFX();
        ShowMainPanel();
        audioSettingsSaveLoad.SaveAudioSettings();
        videoSettingsSaveLoad.ClearTemporarySettings();
    }
    // SETTINGS CONTROL SECTION      END

    // CREDITS CONTROL SECTION      START
    private CanvasGroup creditsCanvasGroup;
    private RectTransform creditsRectTransform;
    private Vector2 creditsStartPosition;
    public void ShowCreditsPanel()
    {
        PlayAcceptSFX();
        SetAllPanelsInactive();
        creditsPanel.SetActive(true);
        StartCoroutine(CreditsSequence());
    }

    private IEnumerator CreditsSequence()
    {
        // Rest Credits Panel Position.
        creditsRectTransform.anchoredPosition = creditsStartPosition;
        creditsCanvasGroup.alpha = 1f;
        // Move/Fade the Credits Panel.
        yield return StartCoroutine(ScrollCreditsDown());
        yield return StartCoroutine(FadeOutCredits());
        // Return to the Main Panel.
        SetAllPanelsInactive();
        mainPanel.SetActive(true);
    }
    
        private IEnumerator ScrollCreditsDown()
    {
        float elapsedTime = 0f;
        Vector2 startPos = creditsStartPosition;

        // Force layout rebuild to ensure proper height calculation
        LayoutRebuilder.ForceRebuildLayoutImmediate(creditsRectTransform);
        Canvas.ForceUpdateCanvases();
        
        // Calculate target position (scroll down by panel height)
        float panelHeight = creditsRectTransform.rect.height;
        Vector2 targetPos = startPos - new Vector2(0, panelHeight*scrollDistance);

        while (elapsedTime < scrollDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / scrollDuration;
            
            // Smooth scrolling using Lerp
            creditsRectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, progress);
            
            yield return null;
        }

        // Ensure final position
        creditsRectTransform.anchoredPosition = targetPos;
    }

    private IEnumerator FadeOutCredits()
    {
        float elapsedTime = 0f;
        float startAlpha = creditsCanvasGroup.alpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeDuration;
            
            creditsCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            
            yield return null;
        }

        // Ensure fully transparent
        creditsCanvasGroup.alpha = 0f;
    }
    // CREDITS CONTROL SECTION      END

    private void ExitApplication()
    {
        PlayDeclineSFX();
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
                Application.Quit();
    #endif
    }
}