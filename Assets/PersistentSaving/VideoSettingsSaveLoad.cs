using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VideoSettingsSaveLoad : MonoBehaviour
{
    private string videoSavePath;
    private string tempVideoSavePath;
    private string thetemporaryDisplayMode;
    private string temporaryAntiAliasingMode;
    private int temporaryFramerate;
    private int[] temporaryResolution;

    [Header("Display Mode Buttons")]
    public Button WindowedMode;
    public Button BorderlessMode;
    public Button FullscreenMode;
    [Header("Display Mode Selection Highlighting")]
    public GameObject WindowedSelected;
    public GameObject WindowedNotSelected;
    public GameObject BorderlessSelected;
    public GameObject BorderlessNotSelected;
    public GameObject FullscreenSelected;
    public GameObject FullscreenNotSelected;

    [Header("Resolution Buttons & Text")]
    public Button ResolutionForward;
    public Button ResolutionBack;
    public TextMeshProUGUI ResolutionText;
    private int[][] ResolutionsArray = new int[][]
    {
    // Some standard + low resolutions for performance saving.
    // Not too many resolutions to not overwhelm the user.
    // One custom resolution will be added if user resolution is not in the array.
    new int[] {640, 480},    // VGA
    new int[] {800, 600},    // SVGA
    new int[] {1280, 720},   // 720p 16:9
    new int[] {1280, 800},   // 720p 16:10
    new int[] {1920, 1080},  // 1080p 16:9
    new int[] {1920, 1200},  // 1080p 16:10
    new int[] {2560, 1080},  // temp 21:9 for testing
    new int[] {2560, 1440},  // 1440p 16:9
    new int[] {2560, 1600},  // 1440p 16:10
    new int[] {3840, 2160},  // 4K
    };
    private int ResolutionIndexPosition;
    private int ResolutionsArrayLength;

    [Header("Resolution Selection Highlighting")]
    public GameObject ResolutionForwardSelected;
    public GameObject ResolutionForwardNotSelected;
    public GameObject ResolutionTextSelected;
    public GameObject ResolutionTextNotSelected;
    public GameObject ResolutionBackSelected;
    public GameObject ResolutionBackNotSelected;

    [Header("Framerate Buttons & Text")]
    public Button FPSForward;
    public Button FPSBack;
    public TextMeshProUGUI FramerateText;
    private int[] FrameratesArray = { 30, 60, 90, 120, 144, 240, -1 };
    private int FramerateIndexPosition;
    private int FrameratesArrayLength;
    [Header("Framerate Selection Highlighting")]
    public GameObject FPSForwardSelected;
    public GameObject FPSForwardNotSelected;
    public GameObject FramerateSelected;
    public GameObject FramerateNotSelected;
    public GameObject FPSBackSelected;
    public GameObject FPSBackNotSelected;

    [Header("Anti-Aliasing Mode Buttons")]
    public Button AAOffMode;
    public Button FXAAMode;
    public Button TAAMode;
    public Button SMAAMode;
    public Button MSAA4xMode;
    public Button MSAA8xMode;
    public Camera thegreatcamera;
    [Header("Anti-Aliasing Mode Selection Highlighting")]
    public GameObject AAOffSelected;
    public GameObject AAOffNotSelected;
    public GameObject FXAASelected;
    public GameObject FXAANotSelected;
    public GameObject TAASelected;
    public GameObject TAANotSelected;
    public GameObject SMAASelected;
    public GameObject SMAANotSelected;
    public GameObject MSAA4xSelected;
    public GameObject MSAA4xNotSelected;
    public GameObject MSAA8xSelected;
    public GameObject MSAA8xNotSelected;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Display Mode buttons - only change display mode, keep others as "don't change" indicators
        WindowedMode.onClick.AddListener(() => SaveTemporaryVideoSettings("windowed", "N/A", 0, new int[] { 0, 0 }));
        BorderlessMode.onClick.AddListener(() => SaveTemporaryVideoSettings("borderless", "N/A", 0, new int[] { 0, 0 }));
        FullscreenMode.onClick.AddListener(() => SaveTemporaryVideoSettings("fullscreen", "N/A", 0, new int[] { 0, 0 }));

        // Anti-Aliasing buttons - only change anti-aliasing
        AAOffMode.onClick.AddListener(() => SaveTemporaryVideoSettings("N/A", "off", 0, new int[] { 0, 0 }));
        FXAAMode.onClick.AddListener(() => SaveTemporaryVideoSettings("N/A", "fxaa", 0, new int[] { 0, 0 }));
        TAAMode.onClick.AddListener(() => SaveTemporaryVideoSettings("N/A", "taa", 0, new int[] { 0, 0 }));
        SMAAMode.onClick.AddListener(() => SaveTemporaryVideoSettings("N/A", "smaa", 0, new int[] { 0, 0 }));
        MSAA4xMode.onClick.AddListener(() => SaveTemporaryVideoSettings("N/A", "msaa4x", 0, new int[] { 0, 0 }));
        MSAA8xMode.onClick.AddListener(() => SaveTemporaryVideoSettings("N/A", "msaa8x", 0, new int[] { 0, 0 }));

        // Framerate buttons - only change framerate
        FrameratesArrayLength = FrameratesArray.Length;
        FPSForward.onClick.AddListener(() => UpdateFramerateIndexPosition(1));
        FPSBack.onClick.AddListener(() => UpdateFramerateIndexPosition(-1));
        //Add an extra framerate option if the user OS set one is not included in the default options array.
        AddExtraFramerateOptionToArray();
        // By default the middle framerate part where the text is should be highlighted.
        SetAllFramerateSelectionsInactive();
        SetMiddleFramerateSelection();

        // Resolution buttons - only change resolution
        ResolutionForward.onClick.AddListener(() => UpdateResolutionIndexPosition(1));
        ResolutionBack.onClick.AddListener(() => UpdateResolutionIndexPosition(-1));
        ResolutionsArrayLength = ResolutionsArray.Length;
        //Add an extra resolution option if the user OS set one is not included in the default options array.
        // AddExtraResolutionOptionToArray(); Already calling this fucntion beforehand in startscreen.cs.
        // By default the middle resolution part where the text is should be highlighted.
        SetAllResolutionSelectionsInactive();
        SetMiddleResolutionSelection();
    }

    // Framerate Updating       Start
    private async void UpdateFramerateIndexPosition(int FramerateIndexChange)
    {
        FramerateIndexPosition += FramerateIndexChange;
        if (FramerateIndexPosition >= FrameratesArrayLength)
        {
            FramerateIndexPosition = 0;
        }
        else if (FramerateIndexPosition < 0)
        {
            FramerateIndexPosition = FrameratesArrayLength - 1;
        }

        SetAllFramerateSelectionsInactive();

        if (FramerateIndexChange == 1)
        {
            SetForwardFramerateSelection();
            await Task.Delay(150);
            SetAllFramerateSelectionsInactive();
            SetMiddleFramerateSelection();
        }
        else if (FramerateIndexChange == -1)
        {
            SetBackFramerateSelection();
            await Task.Delay(150);
            SetAllFramerateSelectionsInactive();
            SetMiddleFramerateSelection();
        }

        SaveTemporaryVideoSettings("N/A", "N/A", FrameratesArray[FramerateIndexPosition], new int[] { 0, 0 });
    }

    private void AddExtraFramerateOptionToArray()
    {

    }
    // Framerate Updating       End

    // Resolution Updating       Start
    private async void UpdateResolutionIndexPosition(int ResolutionIndexChange)
    {
        ResolutionIndexPosition += ResolutionIndexChange;
        if (ResolutionIndexPosition >= ResolutionsArrayLength)
        {
            ResolutionIndexPosition = 0;
        }
        else if (ResolutionIndexPosition < 0)
        {
            ResolutionIndexPosition = ResolutionsArrayLength - 1;
        }

        SetAllResolutionSelectionsInactive();

        if (ResolutionIndexChange == 1)
        {
            SetForwardResolutionSelection();
            await Task.Delay(150);
            SetAllResolutionSelectionsInactive();
            SetMiddleResolutionSelection();
        }
        else if (ResolutionIndexChange == -1)
        {
            SetBackResolutionSelection();
            await Task.Delay(150);
            SetAllResolutionSelectionsInactive();
            SetMiddleResolutionSelection();
        }

        SaveTemporaryVideoSettings("N/A", "N/A", 0, ResolutionsArray[ResolutionIndexPosition]);
    }
    private int CurrentScreenWidth;
    private int CurrentScreenHeight;
    public void AddExtraResolutionOptionToArray()
    {
        // Check if the current OS res given by the users system is already in the default resolution array before adding it to the array.
        // Bool statement true when not in resolution array.
        CurrentScreenWidth = Screen.currentResolution.width;
        CurrentScreenHeight = Screen.currentResolution.height;
        if (!ResolutionsArray.Any(res => res[0] == CurrentScreenWidth && res[1] == CurrentScreenHeight))
        {
            int[][] newArray = new int[ResolutionsArray.Length + 1][];

            for (int i = 0; i < ResolutionsArray.Length; i++)
            {
                newArray[i] = ResolutionsArray[i];
            }
            newArray[ResolutionsArray.Length] = new int[] {CurrentScreenWidth, CurrentScreenHeight};

            // Sort the array by first element (width), then by second element (height)
            newArray = newArray.OrderBy(res => res[0])
                            .ThenBy(res => res[1])
                            .ToArray();
            ResolutionsArray = newArray;

            ResolutionsArrayLength = ResolutionsArray.Length;
        }
    }
    //Resolution Updating       End

    private void Awake()
    {
        // Set the save path relative to the current folder
        string currentDirectory = Path.GetDirectoryName(Application.dataPath);
        videoSavePath = Path.Combine(currentDirectory, "Assets/PersistentSaving/jsonSaveData", "VideoSettingsData.json");
        tempVideoSavePath = Path.Combine(currentDirectory, "Assets/PersistentSaving/jsonSaveData", "TempVideoSettingsData.json");

        // Ensure the directories exist
        string[] paths = { videoSavePath, tempVideoSavePath };
        foreach (string path in paths)
        {
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }

    // Save video settings to JSON (proper save to be used to change URP renderer settings)
    public void SaveVideoSettings()
    {
        // Use the current temporary settings to save the new proper settings which are the previous proper save settings + changes made since.
        VideoSettingsData data = new VideoSettingsData(thetemporaryDisplayMode, temporaryAntiAliasingMode, temporaryFramerate, temporaryResolution);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(videoSavePath, json);

        Debug.Log($"Proper video settings saved to: {videoSavePath}");

        // Apply the settings to the actual game
        ApplyVideoSettings();
    }

    // Load video settings from JSON
    public void LoadVideoSettings()
    {
        if (File.Exists(videoSavePath))
        {
            string json = File.ReadAllText(videoSavePath);
            VideoSettingsData data = JsonUtility.FromJson<VideoSettingsData>(json);

            // Loaded in the current video settings as temporary ones for editing around so the new save is based on the previous proper video settings.
            SaveTemporaryVideoSettings(data.displayMode, data.antiAliasingMode, data.framerate, data.resolutionMode);
            Debug.Log("Proper video settings loaded successfully!");
        }
        else
        {
            // Set default values if no save file exists
            SaveTemporaryVideoSettings("windowed", "msaa4x", 60, new int[] { Screen.currentResolution.width, Screen.currentResolution.height });
            Debug.Log("No saved video settings found. Using defaults.");
        }
    }

    // Apply the video settings to the game
    public void ApplyVideoSettings()
    {
        // Apply framerate second
        Application.targetFrameRate = temporaryFramerate == -1 ? -1 : temporaryFramerate;

        // Apply display mode with an explicit framerate
        FullScreenMode targetMode = FullScreenMode.Windowed; // default

        switch (thetemporaryDisplayMode)
        {
            case "fullscreen":
                targetMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case "borderless":
                targetMode = FullScreenMode.FullScreenWindow;
                break;
            case "windowed":
                targetMode = FullScreenMode.Windowed;
                break;
        }

        // Set resolution with an explicit display mode
        Screen.SetResolution(temporaryResolution[0], temporaryResolution[1], targetMode);

        // Apply Anti-aliasing setting
        UniversalAdditionalCameraData cameraData = thegreatcamera.GetUniversalAdditionalCameraData();
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

        if (urpAsset != null)
        {
            switch (temporaryAntiAliasingMode)
            {
                case "off":
                    urpAsset.msaaSampleCount = 1;
                    cameraData.antialiasing = AntialiasingMode.None;
                    break;
                case "fxaa":
                    urpAsset.msaaSampleCount = 1;
                    cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                    break;
                case "taa":
                    urpAsset.msaaSampleCount = 1;
                    cameraData.antialiasing = AntialiasingMode.TemporalAntiAliasing;
                    break;
                case "smaa":
                    urpAsset.msaaSampleCount = 1;
                    cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    break;
                case "msaa4x":
                    urpAsset.msaaSampleCount = 4;
                    cameraData.antialiasing = AntialiasingMode.None;
                    break;
                case "msaa8x":
                    urpAsset.msaaSampleCount = 8;
                    cameraData.antialiasing = AntialiasingMode.None;
                    break;
            }
        }

        Debug.Log("Video settings applied to the application URP");
    }

    public void SaveTemporaryVideoSettings(string tempDisplayMode, string tempAntiAliasingMode, int tempFramerate, int[] tempResolution)
    {
        // Only update the fields that have valid values (not "N/A")
        if (tempDisplayMode != "N/A")
        {
            thetemporaryDisplayMode = tempDisplayMode;
            SetAllDisplaySelectionsInactive();
            ShowCurrentDisplayModeSelections(tempDisplayMode);
        }
        if (tempAntiAliasingMode != "N/A")
        {
            temporaryAntiAliasingMode = tempAntiAliasingMode;
            SetAllAntiAliasingSelectionsInactive();
            ShowCurrentAntiAliasingModeSelections(tempAntiAliasingMode);
        }
        if (tempFramerate != 0)
        {
            temporaryFramerate = tempFramerate;
            FramerateIndexPosition = Array.IndexOf(FrameratesArray, tempFramerate);
            if (tempFramerate != -1)
            {
                FramerateText.text = tempFramerate.ToString() + " FPS";
            }
            else
            {
                FramerateText.text = "Unlimited";
            }
        }
        if (tempResolution[0] != 0 || tempResolution[1] != 0)
        {
            temporaryResolution = tempResolution;
            ResolutionIndexPosition = -1;
            for (int i = 0; i < ResolutionsArray.Length; i++)
            {
                if (ResolutionsArray[i].SequenceEqual(tempResolution))
                {
                    ResolutionIndexPosition = i;
                    break;
                }
            }
            ResolutionText.text = tempResolution[0].ToString() + "x" + tempResolution[1].ToString();
        }

        VideoSettingsData data = new VideoSettingsData(thetemporaryDisplayMode, temporaryAntiAliasingMode, temporaryFramerate, temporaryResolution);
        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(tempVideoSavePath, json);
    }

    // Public method to get current temporary settings (for UI feedback)
    public VideoSettingsData GetCurrentTemporarySettings()
    {
        return new VideoSettingsData(thetemporaryDisplayMode, temporaryAntiAliasingMode, temporaryFramerate, temporaryResolution);
    }

    // Public method to clear temporary settings
    public void ClearTemporarySettings()
    {
        // Use the current which is the old save settings to overwrite onto the new temporary settings
        // so if anything other than the save changes btn is pressed the temporary changes made are since the old settings are discarded.
        string json = File.ReadAllText(videoSavePath);
        File.WriteAllText(tempVideoSavePath, json);

        Debug.Log($"Cleared changes since user decided not to save changes: {tempVideoSavePath}");
    }

    // For controlling the selection highlight images behind each button in the video settings.
    private void SetAllDisplaySelectionsInactive()
    {
        WindowedSelected.SetActive(false);
        WindowedNotSelected.SetActive(false);
        BorderlessSelected.SetActive(false);
        BorderlessNotSelected.SetActive(false);
        FullscreenSelected.SetActive(false);
        FullscreenNotSelected.SetActive(false);
    }
    public void ShowCurrentDisplayModeSelections(string PassedMode)
    {
        switch (PassedMode)
        {
            case "fullscreen":
                WindowedNotSelected.SetActive(true);
                BorderlessNotSelected.SetActive(true);
                FullscreenSelected.SetActive(true);
                break;
            case "borderless":
                WindowedNotSelected.SetActive(true);
                BorderlessSelected.SetActive(true);
                FullscreenNotSelected.SetActive(true);
                break;
            case "windowed":
                WindowedSelected.SetActive(true);
                BorderlessNotSelected.SetActive(true);
                FullscreenNotSelected.SetActive(true);
                break;
        }
    }

    private void SetAllResolutionSelectionsInactive()
    {
        ResolutionForwardSelected.SetActive(false);
        ResolutionForwardNotSelected.SetActive(false);
        ResolutionTextSelected.SetActive(false);
        ResolutionTextNotSelected.SetActive(false);
        ResolutionBackSelected.SetActive(false);
        ResolutionBackNotSelected.SetActive(false);
    }
    private void SetForwardResolutionSelection()
    {
        ResolutionForwardSelected.SetActive(true);
        ResolutionTextNotSelected.SetActive(true);
        ResolutionBackNotSelected.SetActive(true);
    }
    private void SetMiddleResolutionSelection()
    {
        ResolutionForwardNotSelected.SetActive(true);
        ResolutionTextSelected.SetActive(true);
        ResolutionBackNotSelected.SetActive(true);
    }
    private void SetBackResolutionSelection()
    {
        ResolutionForwardNotSelected.SetActive(true);
        ResolutionTextNotSelected.SetActive(true);
        ResolutionBackSelected.SetActive(true);
    }

    private void SetAllFramerateSelectionsInactive()
    {
        FPSForwardSelected.SetActive(false);
        FPSForwardNotSelected.SetActive(false);
        FramerateSelected.SetActive(false);
        FramerateNotSelected.SetActive(false);
        FPSBackSelected.SetActive(false);
        FPSBackNotSelected.SetActive(false);
    }
    private void SetForwardFramerateSelection()
    {
        FPSForwardSelected.SetActive(true);
        FramerateNotSelected.SetActive(true);
        FPSBackNotSelected.SetActive(true);
    }
    private void SetMiddleFramerateSelection()
    {
        FPSForwardNotSelected.SetActive(true);
        FramerateSelected.SetActive(true);
        FPSBackNotSelected.SetActive(true);
    }
    private void SetBackFramerateSelection()
    {
        FPSForwardNotSelected.SetActive(true);
        FramerateNotSelected.SetActive(true);
        FPSBackSelected.SetActive(true);
    }

    private void SetAllAntiAliasingSelectionsInactive()
    {
        AAOffSelected.SetActive(false);
        AAOffNotSelected.SetActive(false);
        FXAASelected.SetActive(false);
        FXAANotSelected.SetActive(false);
        TAASelected.SetActive(false);
        TAANotSelected.SetActive(false);
        SMAASelected.SetActive(false);
        SMAANotSelected.SetActive(false);
        MSAA4xSelected.SetActive(false);
        MSAA4xNotSelected.SetActive(false);
        MSAA8xSelected.SetActive(false);
        MSAA8xNotSelected.SetActive(false);
    }
    public void ShowCurrentAntiAliasingModeSelections(string PassedMode)
    {
        switch (PassedMode)
        {
            case "off":
                AAOffSelected.SetActive(true);
                FXAANotSelected.SetActive(true);
                TAANotSelected.SetActive(true);
                SMAANotSelected.SetActive(true);
                MSAA4xNotSelected.SetActive(true);
                MSAA8xNotSelected.SetActive(true);
                break;
            case "fxaa":
                AAOffNotSelected.SetActive(true);
                FXAASelected.SetActive(true);
                TAANotSelected.SetActive(true);
                SMAANotSelected.SetActive(true);
                MSAA4xNotSelected.SetActive(true);
                MSAA8xNotSelected.SetActive(true);
                break;
            case "taa":
                AAOffNotSelected.SetActive(true);
                FXAANotSelected.SetActive(true);
                TAASelected.SetActive(true);
                SMAANotSelected.SetActive(true);
                MSAA4xNotSelected.SetActive(true);
                MSAA8xNotSelected.SetActive(true);
                break;
            case "smaa":
                AAOffNotSelected.SetActive(true);
                FXAANotSelected.SetActive(true);
                TAANotSelected.SetActive(true);
                SMAASelected.SetActive(true);
                MSAA4xNotSelected.SetActive(true);
                MSAA8xNotSelected.SetActive(true);
                break;
            case "msaa4x":
                AAOffNotSelected.SetActive(true);
                FXAANotSelected.SetActive(true);
                TAANotSelected.SetActive(true);
                SMAANotSelected.SetActive(true);
                MSAA4xSelected.SetActive(true);
                MSAA8xNotSelected.SetActive(true);
                break;
            case "msaa8x":
                AAOffNotSelected.SetActive(true);
                FXAANotSelected.SetActive(true);
                TAANotSelected.SetActive(true);
                SMAANotSelected.SetActive(true);
                MSAA4xNotSelected.SetActive(true);
                MSAA8xSelected.SetActive(true);
                break;    
        }
    }
}
