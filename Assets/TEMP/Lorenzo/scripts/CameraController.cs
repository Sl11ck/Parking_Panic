using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

    [Header("Camera Bounds")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minBounds = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 maxBounds = new Vector2(50f, 50f);
    [SerializeField] private bool autoCalculateBoundsPadding = true;

    [Header("Parking Focus Settings")]
    [SerializeField] private float normalOrthographicSize = 10f;
    [SerializeField] private float minOrthographicSize = 3f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomPadding = 2f;
    [SerializeField] private float focusBlendFactor = 0.5f;

    [Header("Intro Camera Path")]
    [SerializeField] private bool playIntroOnStart = false;
    [SerializeField] private Transform[] introWaypoints;
    [SerializeField] private float introMoveSpeed = 3f;
    [SerializeField] private float introZoomSize = 15f;
    [SerializeField] private float introWaitTimePerWaypoint = 1f;
    [SerializeField] private float introStartDelay = 0.5f;
    [SerializeField] private bool freezePlayerDuringIntro = true; // NEW
    [SerializeField] private bool hideUIDuringIntro = true; // NEW

    [Header("Intro UI References")] // NEW SECTION
    [SerializeField] private GameObject uiContainer; // Main UI canvas/container to hide
    [SerializeField] private CarController2D carController; // To disable during intro
    [SerializeField] private GearShifting gearShifting; // To disable during intro

    [Header("Debug")]
    [SerializeField] private bool showZoomDebug = false;
    [SerializeField] private bool showBoundsDebug = false;
    [SerializeField] private bool showIntroDebug = true; // NEW

    private Camera cam;
    private bool isFocusingOnParking = false;
    private Transform parkingSpotTransform;
    private Bounds parkingSpotBounds;
    private bool hasParkingSpotBounds = false;
    private float targetOrthographicSize;
    private bool isPlayingIntro = false;
    private bool normalFollowEnabled = true;

    // NEW: Store original state to restore after intro
    private bool carControllerWasEnabled;
    private bool gearShiftingWasEnabled;
    private bool uiWasActive;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
        {
            Debug.LogError("CameraController: No Camera component found!");
            enabled = false;
            return;
        }

        if (!cam.orthographic)
        {
            Debug.LogWarning("CameraController: Camera is not orthographic! Converting to orthographic mode.");
            cam.orthographic = true;
        }

        if (target == null)
        {
            CarController2D car = FindFirstObjectByType<CarController2D>();
            if (car != null)
            {
                target = car.transform;
                Debug.Log("CameraController: Auto-found car target");
            }
            else
            {
                Debug.LogError("CameraController: No target assigned and couldn't find CarController2D!");
            }
        }

        // NEW: Auto-find references if not assigned
        if (carController == null)
        {
            carController = FindFirstObjectByType<CarController2D>();
        }

        if (gearShifting == null)
        {
            gearShifting = FindFirstObjectByType<GearShifting>();
        }

        if (uiContainer == null)
        {
            // Try to find a Canvas tagged as "UI" or named "UI"
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                if (canvas.gameObject.CompareTag("UI") || canvas.gameObject.name.Contains("UI"))
                {
                    uiContainer = canvas.gameObject;
                    if (showIntroDebug)
                    {
                        Debug.Log($"CameraController: Auto-found UI container - {canvas.gameObject.name}");
                    }
                    break;
                }
            }
        }

        targetOrthographicSize = normalOrthographicSize;
        cam.orthographicSize = normalOrthographicSize;

        if (useBounds && autoCalculateBoundsPadding)
        {
            ValidateAndAdjustBounds();
        }

        if (playIntroOnStart && introWaypoints != null && introWaypoints.Length > 0)
        {
            StartCoroutine(PlayIntroSequence());
        }
    }

    void LateUpdate()
    {
        if (isPlayingIntro || !normalFollowEnabled) return;

        if (target == null) return;

        Vector3 desiredPosition;

        if (isFocusingOnParking && parkingSpotTransform != null)
        {
            Vector3 midpoint = Vector3.Lerp(target.position, parkingSpotTransform.position, focusBlendFactor);
            desiredPosition = midpoint + offset;
            targetOrthographicSize = CalculateDynamicZoom();
        }
        else
        {
            desiredPosition = target.position + offset;
            targetOrthographicSize = normalOrthographicSize;
        }

        if (useBounds)
        {
            desiredPosition = ApplyBounds(desiredPosition, targetOrthographicSize);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthographicSize, zoomSpeed * Time.deltaTime);
    }

    private IEnumerator PlayIntroSequence()
    {
        isPlayingIntro = true;
        normalFollowEnabled = false;

        if (showIntroDebug)
        {
            Debug.Log($"CameraController: Starting intro sequence with {introWaypoints.Length} waypoints...");
        }

        // NEW: Freeze player and hide UI
        FreezePlayerForIntro();

        if (introStartDelay > 0)
        {
            yield return new WaitForSeconds(introStartDelay);
        }

        // Zoom out for overview
        float startSize = cam.orthographicSize;
        float elapsed = 0f;
        float zoomDuration = 1f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            cam.orthographicSize = Mathf.Lerp(startSize, introZoomSize, elapsed / zoomDuration);
            yield return null;
        }

        if (showIntroDebug)
        {
            Debug.Log($"CameraController: Zoomed out to {introZoomSize}");
        }

        // Move through waypoints
        int waypointIndex = 0;
        foreach (Transform waypoint in introWaypoints)
        {
            if (waypoint == null)
            {
                if (showIntroDebug)
                {
                    Debug.LogWarning($"CameraController: Waypoint {waypointIndex} is null, skipping...");
                }
                waypointIndex++;
                continue;
            }

            if (showIntroDebug)
            {
                Debug.Log($"CameraController: Moving to waypoint {waypointIndex} - {waypoint.name} at {waypoint.position}");
            }

            Vector3 startPos = transform.position;
            Vector3 targetPos = new Vector3(waypoint.position.x, waypoint.position.y, transform.position.z);

            float distance = Vector3.Distance(startPos, targetPos);
            float duration = distance / introMoveSpeed;
            elapsed = 0f;

            // Move to waypoint
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            // Ensure exact position
            transform.position = targetPos;

            if (showIntroDebug)
            {
                Debug.Log($"CameraController: Reached waypoint {waypointIndex} - {waypoint.name}");
            }

            // Wait at waypoint
            if (introWaitTimePerWaypoint > 0)
            {
                yield return new WaitForSeconds(introWaitTimePerWaypoint);
            }

            waypointIndex++;
        }

        if (showIntroDebug)
        {
            Debug.Log("CameraController: Finished visiting all waypoints, returning to player...");
        }

        // Return to player
        if (target != null)
        {
            Vector3 startPos = transform.position;
            Vector3 targetPos = target.position + offset;
            float duration = 1.5f;
            elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, normalOrthographicSize, t);
                yield return null;
            }

            if (showIntroDebug)
            {
                Debug.Log("CameraController: Returned to player");
            }
        }

        // NEW: Restore player control and UI
        UnfreezePlayerAfterIntro();

        if (showIntroDebug)
        {
            Debug.Log("CameraController: Intro sequence complete!");
        }

        isPlayingIntro = false;
        normalFollowEnabled = true;
    }

    // NEW: Freeze player and hide UI
    private void FreezePlayerForIntro()
    {
        if (freezePlayerDuringIntro)
        {
            if (carController != null)
            {
                carControllerWasEnabled = carController.enabled;
                carController.enabled = false;
                if (showIntroDebug)
                {
                    Debug.Log("CameraController: Disabled CarController for intro");
                }
            }

            if (gearShifting != null)
            {
                gearShiftingWasEnabled = gearShifting.enabled;
                gearShifting.enabled = false;
                if (showIntroDebug)
                {
                    Debug.Log("CameraController: Disabled GearShifting for intro");
                }
            }
        }

        if (hideUIDuringIntro && uiContainer != null)
        {
            uiWasActive = uiContainer.activeSelf;
            uiContainer.SetActive(false);
            if (showIntroDebug)
            {
                Debug.Log("CameraController: Hidden UI container for intro");
            }
        }
    }

    // NEW: Restore player control and UI
    private void UnfreezePlayerAfterIntro()
    {
        if (freezePlayerDuringIntro)
        {
            if (carController != null)
            {
                carController.enabled = carControllerWasEnabled;
                if (showIntroDebug)
                {
                    Debug.Log($"CameraController: Restored CarController (enabled: {carControllerWasEnabled})");
                }
            }

            if (gearShifting != null)
            {
                gearShifting.enabled = gearShiftingWasEnabled;
                if (showIntroDebug)
                {
                    Debug.Log($"CameraController: Restored GearShifting (enabled: {gearShiftingWasEnabled})");
                }
            }
        }

        if (hideUIDuringIntro && uiContainer != null)
        {
            uiContainer.SetActive(uiWasActive);
            if (showIntroDebug)
            {
                Debug.Log($"CameraController: Restored UI container (active: {uiWasActive})");
            }
        }
    }

    private Vector3 ApplyBounds(Vector3 desiredPosition, float orthographicSize)
    {
        float camHeight = orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float minX = minBounds.x + camWidth;
        float maxX = maxBounds.x - camWidth;
        float minY = minBounds.y + camHeight;
        float maxY = maxBounds.y - camHeight;

        if (minX >= maxX || minY >= maxY)
        {
            if (showBoundsDebug)
            {
                Debug.LogWarning($"CameraController: Bounds too small! Camera view ({camWidth * 2}x{camHeight * 2}) doesn't fit in bounds ({maxBounds.x - minBounds.x}x{maxBounds.y - minBounds.y})");
            }

            if (autoCalculateBoundsPadding)
            {
                desiredPosition.x = (minBounds.x + maxBounds.x) / 2f;
                desiredPosition.y = (minBounds.y + maxBounds.y) / 2f;
            }
            return desiredPosition;
        }

        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);

        return desiredPosition;
    }

    private void ValidateAndAdjustBounds()
    {
        float camHeight = normalOrthographicSize;
        float camWidth = camHeight * cam.aspect;

        float requiredWidth = camWidth * 2f;
        float requiredHeight = camHeight * 2f;

        float currentWidth = maxBounds.x - minBounds.x;
        float currentHeight = maxBounds.y - minBounds.y;

        if (currentWidth < requiredWidth || currentHeight < requiredHeight)
        {
            Debug.LogWarning($"CameraController: Bounds ({currentWidth}x{currentHeight}) are smaller than camera view ({requiredWidth}x{requiredHeight}). Consider increasing bounds.");
        }
        else
        {
            Debug.Log($"CameraController: Bounds validated. Camera view: {requiredWidth:F1}x{requiredHeight:F1}, Bounds: {currentWidth:F1}x{currentHeight:F1}");
        }
    }

    private float CalculateDynamicZoom()
    {
        if (target == null || parkingSpotTransform == null)
        {
            if (showZoomDebug) Debug.Log("Dynamic Zoom: Missing target or parking spot");
            return normalOrthographicSize;
        }

        Collider2D playerCollider = target.GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            if (showZoomDebug) Debug.Log("Dynamic Zoom: No player collider found");
            return normalOrthographicSize;
        }

        Bounds playerBounds = playerCollider.bounds;
        Bounds combinedBounds = new Bounds(playerBounds.center, playerBounds.size);

        if (hasParkingSpotBounds)
        {
            combinedBounds.Encapsulate(parkingSpotBounds.min);
            combinedBounds.Encapsulate(parkingSpotBounds.max);

            if (showZoomDebug)
            {
                Debug.Log($"Dynamic Zoom: Player bounds {playerBounds.size}, Parking bounds {parkingSpotBounds.size}");
                Debug.Log($"Dynamic Zoom: Combined bounds size {combinedBounds.size}");
            }
        }
        else
        {
            combinedBounds.Encapsulate(parkingSpotTransform.position);
            if (showZoomDebug) Debug.Log("Dynamic Zoom: Using parking spot position only (no bounds)");
        }

        float requiredHeight = (combinedBounds.size.y / 2f) + zoomPadding;
        float requiredWidth = (combinedBounds.size.x / 2f) + zoomPadding;
        float requiredWidthSize = requiredWidth / cam.aspect;

        float requiredSize = Mathf.Max(requiredHeight, requiredWidthSize);
        float finalSize = Mathf.Max(minOrthographicSize, requiredSize);

        if (showZoomDebug)
        {
            Debug.Log($"Dynamic Zoom: Required H={requiredHeight:F2}, W={requiredWidthSize:F2}, Final={finalSize:F2}, Current={cam.orthographicSize:F2}");
        }

        return finalSize;
    }

    public void StartParkingFocus(Transform parkingSpot, Bounds parkingBounds)
    {
        isFocusingOnParking = true;
        parkingSpotTransform = parkingSpot;
        parkingSpotBounds = parkingBounds;
        hasParkingSpotBounds = true;
        Debug.Log($"CameraController: Started parking focus with bounds - Center: {parkingBounds.center}, Size: {parkingBounds.size}");
    }

    public void StartParkingFocus(Transform parkingSpot)
    {
        isFocusingOnParking = true;
        parkingSpotTransform = parkingSpot;
        hasParkingSpotBounds = false;
        Debug.Log("CameraController: Started parking focus without bounds");
    }

    public void StopParkingFocus()
    {
        isFocusingOnParking = false;
        parkingSpotTransform = null;
        hasParkingSpotBounds = false;
        Debug.Log("CameraController: Stopped parking focus - camera zooming out");
    }

    public void SetBounds(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;

        if (autoCalculateBoundsPadding)
        {
            ValidateAndAdjustBounds();
        }
    }

    public void SetUseBounds(bool use)
    {
        useBounds = use;
    }

    public bool IsFocusingOnParking => isFocusingOnParking;
    public bool IsPlayingIntro => isPlayingIntro; // NEW: Public property to check intro state

    private void OnDrawGizmosSelected()
    {
        if (!useBounds) return;

        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2f, (minBounds.y + maxBounds.y) / 2f, 0f);
        Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0f);
        Gizmos.DrawWireCube(center, size);

        if (cam != null && cam.orthographic)
        {
            float camHeight = normalOrthographicSize;
            float camWidth = camHeight * cam.aspect;

            Gizmos.color = Color.yellow;
            Vector3 constrainedCenter = center;
            Vector3 constrainedSize = new Vector3(
                Mathf.Max(0, size.x - camWidth * 2f),
                Mathf.Max(0, size.y - camHeight * 2f),
                0f
            );
            Gizmos.DrawWireCube(constrainedCenter, constrainedSize);

            Gizmos.color = isFocusingOnParking ? Color.yellow : Color.green;
            float currentCamHeight = Application.isPlaying ? cam.orthographicSize * 2f : camHeight * 2f;
            float currentCamWidth = Application.isPlaying ? cam.orthographicSize * cam.aspect * 2f : camWidth * 2f;
            Gizmos.DrawWireCube(transform.position, new Vector3(currentCamWidth, currentCamHeight, 0f));

            if (Application.isPlaying && isFocusingOnParking && hasParkingSpotBounds)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(parkingSpotBounds.center, parkingSpotBounds.size);
            }
        }

        if (playIntroOnStart && introWaypoints != null && introWaypoints.Length > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < introWaypoints.Length; i++)
            {
                if (introWaypoints[i] == null) continue;

                Vector3 pos = introWaypoints[i].position;
                Gizmos.DrawWireSphere(pos, 0.5f);

#if UNITY_EDITOR
                UnityEditor.Handles.Label(pos + Vector3.up * 0.7f, $"Intro WP {i}");
#endif

                if (i < introWaypoints.Length - 1 && introWaypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(pos, introWaypoints[i + 1].position);
                }
            }
        }
    }
}