using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UI_Script : MonoBehaviour
{
    [Header("Speedometer Settings")]
    [SerializeField] private RectTransform needleTransform;
    [SerializeField] private float minSpeedAngle = 220f; // Angle at 0 speed
    [SerializeField] private float maxSpeedAngle = -40f; // Angle at max speed
    [SerializeField] private float maxSpeedValue = 180f; // Max speed on dial (km/h or mph)

    [Header("Gear Display")]
    [SerializeField] private TextMeshProUGUI gearText; // Text to display current gear (N, R, 1-5)

    [Header("Mistake Checkboard")]
    [SerializeField] private RectTransform checkboardPanel; // Reference to the checkboard's RectTransform
    [SerializeField] private Image checkboardImage;
    [SerializeField] private Sprite bareCheckboardSprite;
    [SerializeField] private Sprite passedCheckboardSprite;
    [SerializeField] private Sprite failedCheckboardSprite;
    [SerializeField] private float scaledUpSize = 2f; // How much to scale up (2 = double size)
    [SerializeField] private float scaleAnimationDuration = 0.5f; // Duration of scale animation
    [SerializeField] private Vector2 centerOffset = Vector2.zero; // Offset from center (e.g., (0, 100) moves up 100 pixels)

    [Header("Mistake Display")]
    [SerializeField] private Transform mistakeMarkerContainer;
    [SerializeField] private GameObject mistakeMarkPrefab; // Visual X marks prefab

    [Header("Game State")]
    [SerializeField] private GameObject passPanel;
    [SerializeField] private GameObject failPanel;

    [Header("References")]
    [SerializeField] private CarController2D carController;
    [SerializeField] private GearShifting gearShifting;

    // (Testing/gameplay state moved to SignManager.)

    [Header("Scene Management")]
    [SerializeField] private SceneReference currentLevelScene; // Scene to reload on retry
    [SerializeField] private SceneReference nextLevelScene; // Next scene to load

    private string currentGear = "0"; // Current gear being displayed

    // Store original checkboard transform values
    private Vector2 originalCheckboardPosition;
    private Vector2 originalCheckboardAnchorMin;
    private Vector2 originalCheckboardAnchorMax;
    private Vector2 originalCheckboardPivot;
    private Vector3 originalCheckboardScale;

    private bool isAnimatingCheckboard = false;

    void Start()
    {
        // Store original checkboard transform
        if (checkboardPanel != null)
        {
            originalCheckboardPosition = checkboardPanel.anchoredPosition;
            originalCheckboardAnchorMin = checkboardPanel.anchorMin;
            originalCheckboardAnchorMax = checkboardPanel.anchorMax;
            originalCheckboardPivot = checkboardPanel.pivot;
            originalCheckboardScale = checkboardPanel.localScale;
        }

        // Initialize UI
        UpdateCheckboard();
        UpdateGearDisplay();

        if (passPanel != null) passPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);

        // Auto-find car controller if not assigned
        if (carController == null)
        {
            carController = FindFirstObjectByType<CarController2D>();
        }

        // Auto-find gear shifting if not assigned
        if (gearShifting == null)
        {
            gearShifting = FindFirstObjectByType<GearShifting>();
        }
    }

    void Update()
    {
        // UI updates each frame (reads current car/gear if references are set)
        UpdateSpeedometer();
        UpdateGear();
    }

    // Compatibility: no-arg version reads car if available
    private void UpdateSpeedometer()
    {
        float speed = GetCarSpeed();
        UpdateSpeedometer(speed);
    }

    // UI-only: caller provides current speed
    public void UpdateSpeedometer(float currentSpeed)
    {
        if (needleTransform == null) return;

        float speedPercentage = Mathf.Clamp01(currentSpeed / maxSpeedValue);
        float targetAngle = Mathf.Lerp(minSpeedAngle, maxSpeedAngle, speedPercentage);

        // Smooth rotation for more realistic movement
        float currentAngle = needleTransform.localEulerAngles.z;
        if (currentAngle > 180f) currentAngle -= 360f;

        float smoothAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * 5f);
        needleTransform.localEulerAngles = new Vector3(0, 0, smoothAngle);
    }

    private void UpdateGear()
    {
        string newGear = GetCurrentGear();

        if (newGear != currentGear)
        {
            currentGear = newGear;
            UpdateGearDisplay();
        }
    }

    // External caller (game logic) can set gear to display using this method
    public void UpdateGear(string newGear)
    {
        if (newGear != currentGear)
        {
            currentGear = newGear;
            UpdateGearDisplay();
        }
    }

    private string GetCurrentGear()
    {
        // Try calling a public GetGear() method if present, otherwise fall back to private field
        if (gearShifting != null)
        {
            var method = typeof(GearShifting).GetMethod("GetGear", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                return (string)method.Invoke(gearShifting, null);
            }

            var fieldInfo = typeof(GearShifting).GetField("currentGear", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldInfo != null)
            {
                return (string)fieldInfo.GetValue(gearShifting);
            }
        }

        return "1"; // Default to first gear if unavailable
    }

    private void UpdateGearDisplay()
    {
        // Update gear text
        if (gearText != null)
        {
            gearText.text = currentGear.ToString();
        }
    }

    private float GetCarSpeed()
    {
        if (carController != null)
        {
            Rigidbody2D rb = carController.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                return rb.linearVelocity.magnitude;
            }
        }
        return 0f;
    }

    // Update the checkboard sprite. `isFailed` true => failed sprite; false => bare.
    public void UpdateCheckboard(bool isFailed)
    {
        if (checkboardImage == null) return;

        checkboardImage.sprite = isFailed ? failedCheckboardSprite : bareCheckboardSprite;
    }

    // Compatibility no-arg UpdateCheckboard
    public void UpdateCheckboard()
    {
        UpdateCheckboard(false);
    }

    // Presentational methods — UI only
    public void ShowPassedUI()
    {
        if (checkboardImage != null && passedCheckboardSprite != null)
        {
            checkboardImage.sprite = passedCheckboardSprite;
        }

        AnimateCheckboardToCenter();

        if (passPanel != null)
        {
            passPanel.SetActive(true);
            carController.gameObject.SetActive(false);
        }
    }

    public void ShowFailedUI()
    {
        if (checkboardImage != null && failedCheckboardSprite != null)
        {
            checkboardImage.sprite = failedCheckboardSprite;
        }

        AnimateCheckboardToCenter();

        if (failPanel != null)
        {
            failPanel.SetActive(true);
            carController.gameObject.SetActive(false);
        }
    }

    public void AddMistakeMark()
    {
        if (mistakeMarkPrefab != null && mistakeMarkerContainer != null)
        {
            Instantiate(mistakeMarkPrefab, mistakeMarkerContainer);
        }
    }

    private void AnimateCheckboardToCenter()
    {
        if (checkboardPanel == null || isAnimatingCheckboard) return;

        StartCoroutine(AnimateCheckboardCoroutine());
    }

    private System.Collections.IEnumerator AnimateCheckboardCoroutine()
    {
        isAnimatingCheckboard = true;

        // Store starting values
        Vector2 startPosition = checkboardPanel.anchoredPosition;
        Vector2 startAnchorMin = checkboardPanel.anchorMin;
        Vector2 startAnchorMax = checkboardPanel.anchorMax;
        Vector3 startScale = checkboardPanel.localScale;

        // Target values (centered with offset and scaled up)
        Vector2 targetAnchorMin = new Vector2(0.5f, 0.5f);
        Vector2 targetAnchorMax = new Vector2(0.5f, 0.5f);
        Vector2 targetPivot = new Vector2(0.5f, 0.5f);
        Vector2 targetPosition = centerOffset; // Apply the offset from center
        Vector3 targetScale = originalCheckboardScale * scaledUpSize;

        float elapsed = 0f;

        while (elapsed < scaleAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleAnimationDuration);

            // Ease out effect for smoother animation
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            // Animate anchors
            checkboardPanel.anchorMin = Vector2.Lerp(startAnchorMin, targetAnchorMin, smoothT);
            checkboardPanel.anchorMax = Vector2.Lerp(startAnchorMax, targetAnchorMax, smoothT);

            // Animate pivot
            checkboardPanel.pivot = Vector2.Lerp(checkboardPanel.pivot, targetPivot, smoothT);

            // Animate position
            checkboardPanel.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, smoothT);

            // Animate scale
            checkboardPanel.localScale = Vector3.Lerp(startScale, targetScale, smoothT);

            yield return null;
        }

        // Ensure final values are set
        checkboardPanel.anchorMin = targetAnchorMin;
        checkboardPanel.anchorMax = targetAnchorMax;
        checkboardPanel.pivot = targetPivot;
        checkboardPanel.anchoredPosition = targetPosition;
        checkboardPanel.localScale = targetScale;

        isAnimatingCheckboard = false;
    }

    /// <summary>
    /// Reset the test (for retry functionality)
    /// </summary>
    public void ResetTest()
    {
        currentGear = "0";

        // Reset checkboard to original position and scale
        if (checkboardPanel != null)
        {
            StopAllCoroutines(); // Stop any ongoing animation
            isAnimatingCheckboard = false;

            checkboardPanel.anchoredPosition = originalCheckboardPosition;
            checkboardPanel.anchorMin = originalCheckboardAnchorMin;
            checkboardPanel.anchorMax = originalCheckboardAnchorMax;
            checkboardPanel.pivot = originalCheckboardPivot;
            checkboardPanel.localScale = originalCheckboardScale;
        }

        UpdateCheckboard();
        UpdateGearDisplay();

        // Clear all mistake markers
        if (mistakeMarkerContainer != null)
        {
            foreach (Transform child in mistakeMarkerContainer)
            {
                Destroy(child.gameObject);
            }
        }

        if (passPanel != null) passPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);
    }

    /// <summary>
    /// Reloads the configured current level (for retry button)
    /// </summary>
    public void RetryCurrentLevel()
    {
        if (currentLevelScene != null && !string.IsNullOrEmpty(currentLevelScene.SceneName))
        {
            SceneManager.LoadScene(currentLevelScene.SceneName);
        }
        else
        {
            Debug.LogError("UI_Script: Current level scene is not assigned!");
        }
    }

    /// <summary>
    /// Loads the configured next level
    /// </summary>
    public void LoadConfiguredNextLevel()
    {
        if (nextLevelScene != null && !string.IsNullOrEmpty(nextLevelScene.SceneName))
        {
            SceneManager.LoadScene(nextLevelScene.SceneName);
        }
        else
        {
            Debug.LogError("UI_Script: Next level scene is not assigned!");
        }
    }
}

[System.Serializable]
public class SceneReference
{
#if UNITY_EDITOR
    [SerializeField] private Object sceneAsset;
#endif
    [SerializeField] private string sceneName;

    public string SceneName => sceneName;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sceneAsset != null)
        {
            string path = AssetDatabase.GetAssetPath(sceneAsset);
            sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
        }
        else
        {
            sceneName = string.Empty;
        }
    }
#endif
}