using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraPath : MonoBehaviour
{
    [System.Serializable]
    public class PathWaypoint
    {
        public Transform transform;
        public float orthographicSize = 10f;
        public float waitTime = 1f; // Time to pause at this waypoint
        
        [Tooltip("If true, camera will look at this point while moving to next waypoint")]
        public bool lookAtWhileMoving = false;
    }

    [Header("Path Settings")]
    [SerializeField] private List<PathWaypoint> waypoints = new List<PathWaypoint>();
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private bool loopPath = false;
    [SerializeField] private bool smoothPath = true; // Use smooth interpolation
    
    [Header("Auto Play")]
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private float startDelay = 0f;
    
    [Header("References")]
    [SerializeField] private CameraController cameraController;
    
    [Header("Debug")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color pathColor = Color.green;
    [SerializeField] private Color waypointColor = Color.yellow;
    
    private Camera cam;
    private bool isPlayingPath = false;
    private int currentWaypointIndex = 0;
    private bool cameraControllerWasEnabled = false;

    void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("CameraPath: No main camera found!");
            enabled = false;
            return;
        }

        // Auto-find camera controller if not assigned
        if (cameraController == null)
        {
            cameraController = cam.GetComponent<CameraController>();
        }

        if (playOnStart)
        {
            if (startDelay > 0)
            {
                Invoke(nameof(PlayPath), startDelay);
            }
            else
            {
                PlayPath();
            }
        }
    }

    /// <summary>
    /// Start playing the camera path
    /// </summary>
    public void PlayPath()
    {
        if (waypoints.Count == 0)
        {
            Debug.LogWarning("CameraPath: No waypoints defined!");
            return;
        }

        if (isPlayingPath)
        {
            Debug.LogWarning("CameraPath: Already playing path!");
            return;
        }

        StartCoroutine(PlayPathCoroutine());
    }

    /// <summary>
    /// Stop playing the path and return control to camera controller
    /// </summary>
    public void StopPath()
    {
        StopAllCoroutines();
        isPlayingPath = false;
        
        // Re-enable camera controller
        if (cameraController != null && cameraControllerWasEnabled)
        {
            cameraController.enabled = true;
            if (showDebug)
            {
                Debug.Log("CameraPath: Returned control to CameraController");
            }
        }
    }

    private IEnumerator PlayPathCoroutine()
    {
        isPlayingPath = true;

        // Disable camera controller to take manual control
        if (cameraController != null)
        {
            cameraControllerWasEnabled = cameraController.enabled;
            cameraController.enabled = false;
            if (showDebug)
            {
                Debug.Log("CameraPath: Disabled CameraController for path playback");
            }
        }

        currentWaypointIndex = 0;

        do
        {
            for (int i = 0; i < waypoints.Count; i++)
            {
                currentWaypointIndex = i;
                PathWaypoint waypoint = waypoints[i];

                if (waypoint.transform == null)
                {
                    Debug.LogWarning($"CameraPath: Waypoint {i} has no transform!");
                    continue;
                }

                if (showDebug)
                {
                    Debug.Log($"CameraPath: Moving to waypoint {i} - {waypoint.transform.name}");
                }

                // Move to waypoint
                yield return MoveToWaypoint(waypoint, i < waypoints.Count - 1 ? waypoints[i + 1] : null);

                // Wait at waypoint
                if (waypoint.waitTime > 0)
                {
                    if (showDebug)
                    {
                        Debug.Log($"CameraPath: Waiting {waypoint.waitTime}s at waypoint {i}");
                    }
                    yield return new WaitForSeconds(waypoint.waitTime);
                }
            }

            if (showDebug && loopPath)
            {
                Debug.Log("CameraPath: Looping path...");
            }

        } while (loopPath);

        // Path complete
        isPlayingPath = false;

        if (showDebug)
        {
            Debug.Log("CameraPath: Path complete!");
        }

        // Re-enable camera controller
        if (cameraController != null && cameraControllerWasEnabled)
        {
            cameraController.enabled = true;
            if (showDebug)
            {
                Debug.Log("CameraPath: Returned control to CameraController");
            }
        }
    }

    private IEnumerator MoveToWaypoint(PathWaypoint waypoint, PathWaypoint nextWaypoint)
    {
        Vector3 startPos = cam.transform.position;
        Vector3 targetPos = new Vector3(waypoint.transform.position.x, waypoint.transform.position.y, cam.transform.position.z);
        
        float startSize = cam.orthographicSize;
        float targetSize = waypoint.orthographicSize;

        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Apply smoothing if enabled
            if (smoothPath)
            {
                t = Mathf.SmoothStep(0f, 1f, t);
            }

            // Move camera position
            cam.transform.position = Vector3.Lerp(startPos, targetPos, t);

            // Adjust zoom
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, t * zoomSpeed);

            // Optional: look at next waypoint while moving
            if (waypoint.lookAtWhileMoving && nextWaypoint != null && nextWaypoint.transform != null)
            {
                // For 2D orthographic, we don't rotate, but this could be used for future enhancements
            }

            yield return null;
        }

        // Ensure we reach exact position
        cam.transform.position = targetPos;
        cam.orthographicSize = targetSize;
    }

    /// <summary>
    /// Check if the path is currently playing
    /// </summary>
    public bool IsPlayingPath => isPlayingPath;

    /// <summary>
    /// Get the current waypoint index
    /// </summary>
    public int CurrentWaypointIndex => currentWaypointIndex;

    /// <summary>
    /// Add a waypoint at runtime
    /// </summary>
    public void AddWaypoint(Transform waypointTransform, float orthographicSize = 10f, float waitTime = 1f)
    {
        waypoints.Add(new PathWaypoint
        {
            transform = waypointTransform,
            orthographicSize = orthographicSize,
            waitTime = waitTime
        });
    }

    /// <summary>
    /// Clear all waypoints
    /// </summary>
    public void ClearWaypoints()
    {
        waypoints.Clear();
    }

    // Visualize path in editor
    private void OnDrawGizmos()
    {
        if (!showGizmos || waypoints.Count == 0) return;

        // Draw waypoints
        for (int i = 0; i < waypoints.Count; i++)
        {
            PathWaypoint waypoint = waypoints[i];
            if (waypoint.transform == null) continue;

            Vector3 pos = waypoint.transform.position;

            // Draw waypoint sphere
            Gizmos.color = waypointColor;
            Gizmos.DrawWireSphere(pos, 0.5f);

            // Draw waypoint number
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(pos + Vector3.up * 0.7f, $"WP {i}\nWait: {waypoint.waitTime}s\nZoom: {waypoint.orthographicSize}");
            #endif

            // Draw camera view bounds at this waypoint
            Gizmos.color = new Color(waypointColor.r, waypointColor.g, waypointColor.b, 0.3f);
            Camera cam = Camera.main;
            if (cam != null && cam.orthographic)
            {
                float aspect = cam.aspect;
                float height = waypoint.orthographicSize * 2f;
                float width = height * aspect;
                Gizmos.DrawWireCube(pos, new Vector3(width, height, 0f));
            }

            // Draw line to next waypoint
            if (i < waypoints.Count - 1 && waypoints[i + 1].transform != null)
            {
                Gizmos.color = pathColor;
                Gizmos.DrawLine(pos, waypoints[i + 1].transform.position);

                // Draw arrow direction
                Vector3 direction = (waypoints[i + 1].transform.position - pos).normalized;
                Vector3 midPoint = (pos + waypoints[i + 1].transform.position) / 2f;
                DrawArrow(midPoint, direction, 0.5f);
            }
            else if (loopPath && i == waypoints.Count - 1 && waypoints[0].transform != null)
            {
                // Draw loop back to start
                Gizmos.color = new Color(pathColor.r, pathColor.g, pathColor.b, 0.5f);
                Gizmos.DrawLine(pos, waypoints[0].transform.position);
            }
        }

        // Highlight current waypoint during playback
        if (Application.isPlaying && isPlayingPath && currentWaypointIndex < waypoints.Count)
        {
            PathWaypoint current = waypoints[currentWaypointIndex];
            if (current.transform != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(current.transform.position, 0.7f);
            }
        }
    }

    private void DrawArrow(Vector3 position, Vector3 direction, float size)
    {
        Vector3 right = Vector3.Cross(direction, Vector3.forward).normalized * size * 0.3f;
        Vector3 arrowTip = position + direction * size * 0.3f;
        
        Gizmos.DrawLine(arrowTip, position - direction * size * 0.3f + right);
        Gizmos.DrawLine(arrowTip, position - direction * size * 0.3f - right);
    }
}