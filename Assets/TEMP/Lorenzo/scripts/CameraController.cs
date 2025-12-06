using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target; // The player/car to follow
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

    [Header("Camera Bounds")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minBounds = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 maxBounds = new Vector2(50f, 50f);

    [Header("Parking Focus Settings")]
    [SerializeField] private float normalOrthographicSize = 10f;
    [SerializeField] private float minOrthographicSize = 3f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomPadding = 2f; // Extra padding around objects
    [SerializeField] private float focusBlendFactor = 0.5f; // 0.5 = halfway between player and parking spot

    [Header("Debug")]
    [SerializeField] private bool showZoomDebug = false;

    private Camera cam;
    private bool isFocusingOnParking = false;
    private Transform parkingSpotTransform;
    private Bounds parkingSpotBounds;
    private bool hasParkingSpotBounds = false;
    private float targetOrthographicSize;

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

        // Auto-find player if not assigned
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

        targetOrthographicSize = normalOrthographicSize;
        cam.orthographicSize = normalOrthographicSize;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Determine target position and zoom
        Vector3 desiredPosition;

        if (isFocusingOnParking && parkingSpotTransform != null)
        {
            // Calculate midpoint between player and parking spot
            Vector3 midpoint = Vector3.Lerp(target.position, parkingSpotTransform.position, focusBlendFactor);
            desiredPosition = midpoint + offset;

            // Dynamically calculate zoom to fit both player and parking spot
            targetOrthographicSize = CalculateDynamicZoom();
        }
        else
        {
            // Normal follow mode
            desiredPosition = target.position + offset;
            targetOrthographicSize = normalOrthographicSize;
        }

        // Apply bounds if enabled
        if (useBounds)
        {
            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x + camWidth, maxBounds.x - camWidth);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y + camHeight, maxBounds.y - camHeight);
        }

        // Smoothly move camera
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Smoothly adjust zoom
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthographicSize, zoomSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Calculate the required orthographic size to fit both player and parking spot
    /// </summary>
    private float CalculateDynamicZoom()
    {
        if (target == null || parkingSpotTransform == null)
        {
            if (showZoomDebug) Debug.Log("Dynamic Zoom: Missing target or parking spot");
            return normalOrthographicSize;
        }

        // Get player bounds
        Collider2D playerCollider = target.GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            if (showZoomDebug) Debug.Log("Dynamic Zoom: No player collider found");
            return normalOrthographicSize;
        }

        Bounds playerBounds = playerCollider.bounds;

        // Create a combined bounds that encompasses both player and parking spot
        Bounds combinedBounds = new Bounds(playerBounds.center, playerBounds.size);

        if (hasParkingSpotBounds)
        {
            // Encapsulate the entire parking spot bounds
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
            // Fallback: use parking spot position
            combinedBounds.Encapsulate(parkingSpotTransform.position);
            if (showZoomDebug) Debug.Log("Dynamic Zoom: Using parking spot position only (no bounds)");
        }

        // Calculate required orthographic size based on the combined bounds
        // For height: orthographicSize is half the height of the view
        float requiredHeight = (combinedBounds.size.y / 2f) + zoomPadding;

        // For width: need to account for aspect ratio
        // orthographicSize * aspect * 2 = view width
        float requiredWidth = (combinedBounds.size.x / 2f) + zoomPadding;
        float requiredWidthSize = requiredWidth / cam.aspect;

        // Use the larger of the two to ensure everything fits
        float requiredSize = Mathf.Max(requiredHeight, requiredWidthSize);

        // Clamp to minimum size
        float finalSize = Mathf.Max(minOrthographicSize, requiredSize);

        if (showZoomDebug)
        {
            Debug.Log($"Dynamic Zoom: Required H={requiredHeight:F2}, W={requiredWidthSize:F2}, Final={finalSize:F2}, Current={cam.orthographicSize:F2}");
        }

        return finalSize;
    }

    /// <summary>
    /// Call this when the player enters the parking zone with bounds information
    /// </summary>
    public void StartParkingFocus(Transform parkingSpot, Bounds parkingBounds)
    {
        isFocusingOnParking = true;
        parkingSpotTransform = parkingSpot;
        parkingSpotBounds = parkingBounds;
        hasParkingSpotBounds = true;
        Debug.Log($"CameraController: Started parking focus with bounds - Center: {parkingBounds.center}, Size: {parkingBounds.size}");
    }

    /// <summary>
    /// Call this when the player enters the parking zone (fallback without bounds)
    /// </summary>
    public void StartParkingFocus(Transform parkingSpot)
    {
        isFocusingOnParking = true;
        parkingSpotTransform = parkingSpot;
        hasParkingSpotBounds = false;
        Debug.Log("CameraController: Started parking focus without bounds");
    }

    /// <summary>
    /// Call this to return to normal follow mode
    /// </summary>
    public void StopParkingFocus()
    {
        isFocusingOnParking = false;
        parkingSpotTransform = null;
        hasParkingSpotBounds = false;
        Debug.Log("CameraController: Stopped parking focus - camera zooming out");
    }

    /// <summary>
    /// Set custom camera bounds at runtime
    /// </summary>
    public void SetBounds(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;
    }

    /// <summary>
    /// Enable or disable bounds checking
    /// </summary>
    public void SetUseBounds(bool use)
    {
        useBounds = use;
    }

    /// <summary>
    /// Check if currently focusing on parking
    /// </summary>
    public bool IsFocusingOnParking => isFocusingOnParking;

    // Visualize bounds in editor
    private void OnDrawGizmosSelected()
    {
        if (!useBounds) return;

        // Draw world bounds
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2f, (minBounds.y + maxBounds.y) / 2f, 0f);
        Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0f);
        Gizmos.DrawWireCube(center, size);

        // Draw current camera view
        if (Application.isPlaying && cam != null && cam.orthographic)
        {
            Gizmos.color = isFocusingOnParking ? Color.yellow : Color.green;
            float camHeight = cam.orthographicSize * 2f;
            float camWidth = camHeight * cam.aspect;
            Gizmos.DrawWireCube(transform.position, new Vector3(camWidth, camHeight, 0f));

            // Draw parking spot bounds if focusing
            if (isFocusingOnParking && hasParkingSpotBounds)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(parkingSpotBounds.center, parkingSpotBounds.size);
            }
        }
    }
}