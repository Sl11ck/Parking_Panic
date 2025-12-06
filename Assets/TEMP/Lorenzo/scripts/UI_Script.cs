using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UI_Script : MonoBehaviour
{
    [Header("Speedometer Settings")]
    [SerializeField] private RectTransform needleTransform;
    [SerializeField] private float minSpeedAngle = 220f; // Angle at 0 speed
    [SerializeField] private float maxSpeedAngle = -40f; // Angle at max speed
    [SerializeField] private float maxSpeedValue = 180f; // Max speed on dial (km/h or mph)

    [Header("Gear Display")]
    [SerializeField] private TextMeshProUGUI gearText; // Text to display current gear (N, R, 1-5)
    [SerializeField] private float[] gearSpeedThresholds = { 0f, 15f, 30f, 50f, 70f }; // Speed thresholds for each gear

    [Header("Mistake Checkboard")]
    [SerializeField] private RectTransform checkboardPanel; // Reference to the checkboard's RectTransform
    [SerializeField] private Image checkboardImage;
    [SerializeField] private Sprite bareCheckboardSprite;
    [SerializeField] private Sprite passedCheckboardSprite;
    [SerializeField] private Sprite failedCheckboardSprite;
    [SerializeField] private int maxMistakes = 5;
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

    [Header("Testing Mode")]
    [SerializeField] private bool enableTestMode = false;
    [SerializeField][Range(-80f, 80f)] private float testSpeed = 0f;

    [Header("Test Controls")]
    [SerializeField] private bool testAddMistake = false; // Check this to add a mistake
    [SerializeField] private bool testCompleteObjective = false; // Check this to complete objective
    [SerializeField] private bool testResetTest = false; // Check this to reset the test

    [Header("Scene Management")]
    [SerializeField] private string currentLevelSceneName = "Level1"; // Scene to reload on retry
    [SerializeField] private string nextLevelSceneName = "Level2"; // Next scene to load

    private int currentMistakes = 0;
    private bool testEnded = false;
    private int currentGear = 0; // 0 = Neutral, 1-5 = Gears, -1 = Reverse

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
    }

    void Update()
    {
        if (!testEnded)
        {
            UpdateSpeedometer();
            UpdateGear();
        }

        // Handle test controls
        HandleTestControls();
    }

    private void HandleTestControls()
    {
        // Test adding mistakes
        if (testAddMistake)
        {
            testAddMistake = false; // Reset the toggle
            RegisterMistake();
            Debug.Log($"Test: Added mistake. Current mistakes: {currentMistakes}/{maxMistakes}");
        }

        // Test completing objective
        if (testCompleteObjective)
        {
            testCompleteObjective = false; // Reset the toggle
            CompleteObjective();
            Debug.Log("Test: Completed objective");
        }

        // Test resetting
        if (testResetTest)
        {
            testResetTest = false; // Reset the toggle
            ResetTest();
            Debug.Log("Test: Reset test");
        }
    }

    private void UpdateSpeedometer()
    {
        if (needleTransform == null) return;

        // Get current speed (already returns magnitude)
        float currentSpeed = GetCarSpeed();

        // Calculate needle angle based on speed (no need to use Abs since GetCarSpeed returns magnitude)
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
        // Get signed speed for gear calculation
        float currentSpeed = GetCarSpeedSigned();
        int newGear = CalculateGear(currentSpeed);

        // Only update if gear changed
        if (newGear != currentGear)
        {
            currentGear = newGear;
            UpdateGearDisplay();
        }
    }

    private int CalculateGear(float speed)
    {
        // Check for reverse
        if (speed < -0.5f)
        {
            return -1; // Reverse gear
        }

        // Check for neutral (very low speed)
        if (Mathf.Abs(speed) < 0.5f)
        {
            return 0; // Neutral
        }

        // Calculate forward gear based on speed thresholds
        float absSpeed = Mathf.Abs(speed);
        for (int i = gearSpeedThresholds.Length - 1; i >= 0; i--)
        {
            if (absSpeed >= gearSpeedThresholds[i])
            {
                return Mathf.Min(i + 1, 5); // Cap at gear 5
            }
        }

        return 1; // Default to first gear
    }

    private void UpdateGearDisplay()
    {
        // Update gear text
        if (gearText != null)
        {
            switch (currentGear)
            {
                case -1:
                    gearText.text = "R";
                    break;
                case 0:
                    gearText.text = "N";
                    break;
                default:
                    gearText.text = currentGear.ToString();
                    break;
            }
        }
    }

    private float GetCarSpeed()
    {
        // Returns magnitude (absolute value) for speedometer display
        if (enableTestMode)
        {
            return Mathf.Abs(testSpeed);
        }

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

    private float GetCarSpeedSigned()
    {
        // Returns signed speed (positive/negative) for gear calculation
        if (enableTestMode)
        {
            return testSpeed;
        }

        if (carController != null)
        {
            Rigidbody2D rb = carController.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // For actual car, use velocity magnitude (always positive)
                // You may need to track direction separately if implementing reverse in real gameplay
                return rb.linearVelocity.magnitude;
            }
        }
        return 0f;
    }

    /// <summary>
    /// Call this method when the player makes a mistake
    /// </summary>
    public void RegisterMistake()
    {
        if (testEnded) return;

        currentMistakes++;
        UpdateCheckboard();

        // Add visual mistake marker
        if (mistakeMarkPrefab != null && mistakeMarkerContainer != null)
        {
            Instantiate(mistakeMarkPrefab, mistakeMarkerContainer);
        }

        // Check if player has failed
        if (currentMistakes >= maxMistakes)
        {
            FailTest();
        }
    }

    /// <summary>
    /// Call this method when the player reaches the objective
    /// </summary>
    public void CompleteObjective()
    {
        if (testEnded) return;

        if (currentMistakes < maxMistakes)
        {
            PassTest();
        }
        else
        {
            FailTest();
        }
    }

    private void UpdateCheckboard()
    {
        if (checkboardImage == null) return;

        if (currentMistakes >= maxMistakes)
        {
            checkboardImage.sprite = failedCheckboardSprite;
        }
        else
        {
            checkboardImage.sprite = bareCheckboardSprite;
        }
    }

    private void PassTest()
    {
        testEnded = true;

        if (checkboardImage != null && passedCheckboardSprite != null)
        {
            checkboardImage.sprite = passedCheckboardSprite;
        }

        // Animate checkboard to center and scale up
        AnimateCheckboardToCenter();

        if (passPanel != null)
        {
            passPanel.SetActive(true);
        }

        Debug.Log("Test Passed!");
    }

    private void FailTest()
    {
        testEnded = true;

        if (checkboardImage != null && failedCheckboardSprite != null)
        {
            checkboardImage.sprite = failedCheckboardSprite;
        }

        // Animate checkboard to center and scale up
        AnimateCheckboardToCenter();

        if (failPanel != null)
        {
            failPanel.SetActive(true);
        }

        Debug.Log("Test Failed!");
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
        currentMistakes = 0;
        testEnded = false;
        currentGear = 0;

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
        if (!string.IsNullOrEmpty(currentLevelSceneName))
        {
            SceneManager.LoadScene(currentLevelSceneName);
        }
        else
        {
            Debug.LogError("Current level scene name not configured!");
        }
    }

    /// <summary>
    /// Loads the configured next level
    /// </summary>
    public void LoadConfiguredNextLevel()
    {
        if (!string.IsNullOrEmpty(nextLevelSceneName))
        {
            SceneManager.LoadScene(nextLevelSceneName);
        }
        else
        {
            Debug.LogError("Next level scene name not configured!");
        }
    }

    // Public accessors
    public int CurrentMistakes => currentMistakes;
    public int MaxMistakes => maxMistakes;
    public bool IsTestEnded => testEnded;
    public int CurrentGear => currentGear;
}