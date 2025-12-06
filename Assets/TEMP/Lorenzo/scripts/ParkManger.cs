using UnityEngine;

public class ParkManger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UI_Script uiScript;
    [SerializeField] private CarController2D carController;
    [SerializeField] private GearShifting gearShifting;
    [SerializeField] private CameraController cameraController;

    [SerializeField] private SignManager signManager;

    [Header("Visual Effects")]
    [SerializeField] private ParkingLightWall lightWallEffect; // Add this reference

    [Header("Collider References")]
    [SerializeField] private BoxCollider2D parkingLotSpaceCollider; // Large area for camera trigger
    [SerializeField] private BoxCollider2D parkingSpaceCollider; // Small area for win condition

    [Header("Parking Requirements")]
    [SerializeField] private float requiredStayTime = 2f; // Time player must stay in the parking spot

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private bool playerInParkingLot = false;
    private bool playerInParkingSpace = false;
    private float timeInZone = 0f;
    private bool parkingCompleted = false;
    private bool cameraFocusActivated = false;
    private Rigidbody2D carRigidbody;
    private Collider2D carCollider;

    void Start()
    {
        // Auto-find UI script if not assigned
        if (uiScript == null)
        {
            uiScript = FindFirstObjectByType<UI_Script>();
        }

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

        // Auto-find camera controller if not assigned
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<CameraController>();
            if (cameraController == null && showDebugInfo)
            {
                Debug.LogWarning("ParkManager: CameraController not found! Camera zoom will not work. Add CameraController script to your Main Camera.");
            }
        }

        // Auto-find light wall effect if not assigned
        if (lightWallEffect == null)
        {
            lightWallEffect = GetComponentInChildren<ParkingLightWall>();
        }

        // Get the car's rigidbody and collider
        if (carController != null)
        {
            carRigidbody = carController.GetComponent<Rigidbody2D>();
            carCollider = carController.GetComponent<Collider2D>();
        }

        // Setup parking lot space collider (camera trigger area)
        if (parkingLotSpaceCollider == null)
        {
            // Try to find it on this GameObject
            parkingLotSpaceCollider = GetComponent<BoxCollider2D>();

            if (parkingLotSpaceCollider != null)
            {
                if (showDebugInfo)
                {
                    Debug.Log("ParkManager: Using BoxCollider2D on this GameObject as parking lot space");
                }
            }
        }

        if (parkingLotSpaceCollider != null)
        {
            parkingLotSpaceCollider.isTrigger = true;
        }
        else
        {
            Debug.LogError("ParkManager: No parking lot space BoxCollider2D found! Please assign one in the inspector.");
        }

        // Setup parking space collider (win condition area)
        if (parkingSpaceCollider == null)
        {
            Debug.LogError("ParkManager: No parking space BoxCollider2D assigned! Please assign the win condition collider in the inspector.");
        }
        else
        {
            parkingSpaceCollider.isTrigger = true;
        }
    }

    void Update()
    {
        if (parkingCompleted) return;

        // Check if car is fully inside the parking SPACE (win condition)
        bool isFullyInside = IsCarFullyInsideParkingSpace();

        // Update parking space status
        if (isFullyInside)
        {
            if (!playerInParkingSpace)
            {
                // Car just became fully inside
                playerInParkingSpace = true;
                timeInZone = 0f;

                // Start the light wall shrinking effect
                if (lightWallEffect != null)
                {
                    lightWallEffect.StartShrinking();
                }

                if (showDebugInfo)
                {
                    Debug.Log("Car is now fully inside parking SPACE - timer started");
                }
            }

            // Increment timer
            timeInZone += Time.deltaTime;

            if (showDebugInfo)
            {
                Debug.Log($"Parking... {timeInZone:F1}s / {requiredStayTime}s");
            }

            // Check if player has stayed long enough
            if (timeInZone >= requiredStayTime)
            {
                CompleteParkingObjective();
            }
        }
        else
        {
            // Car is not fully inside
            if (playerInParkingSpace)
            {
                playerInParkingSpace = false;
                timeInZone = 0f;

                // Reset the light wall
                if (lightWallEffect != null)
                {
                    lightWallEffect.ResetWalls();
                }

                if (showDebugInfo)
                {
                    Debug.Log("Car left parking SPACE or not fully inside - timer reset");
                }
            }
        }
    }

    private bool IsCarFullyInsideParkingSpace()
    {
        if (carCollider == null || parkingSpaceCollider == null)
            return false;

        // Get the bounds of both colliders
        Bounds carBounds = carCollider.bounds;
        Bounds parkingBounds = parkingSpaceCollider.bounds;

        // Check if all corners of the car's bounds are inside the parking space bounds
        bool isFullyContained =
            carBounds.min.x >= parkingBounds.min.x &&
            carBounds.max.x <= parkingBounds.max.x &&
            carBounds.min.y >= parkingBounds.min.y &&
            carBounds.max.y <= parkingBounds.max.y;

        return isFullyContained;
    }

    private void CompleteParkingObjective()
    {
        parkingCompleted = true;

        Debug.Log("Parking completed successfully!");

        // Freeze the car
        FreezePlayer();

        // Notify the UI script that the objective is complete
        if (signManager != null)
        {
            signManager.CompleteObjective();
        }
        else
        {
            Debug.LogError("ParkManager: Sign Manager reference is missing!");
        }
    }

    private void FreezePlayer()
    {
        // Disable car controller to stop input
        if (carController != null)
        {
            carController.enabled = false;
            if (showDebugInfo)
            {
                Debug.Log("CarController disabled");
            }
        }

        // Disable gear shifting to stop gear changes
        if (gearShifting != null)
        {
            gearShifting.enabled = false;
            if (showDebugInfo)
            {
                Debug.Log("GearShifting disabled");
            }
        }

        // Freeze the rigidbody to prevent any drift
        if (carRigidbody != null)
        {
            carRigidbody.linearVelocity = Vector2.zero;
            carRigidbody.angularVelocity = 0f;
            carRigidbody.bodyType = RigidbodyType2D.Kinematic;
            if (showDebugInfo)
            {
                Debug.Log("Rigidbody2D frozen");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the car
        if (other.CompareTag("Player") || other.GetComponent<CarController2D>() != null)
        {
            // Check which collider triggered - need to compare with the collider that triggered this event
            // This trigger is on the parking lot space collider
            if (parkingLotSpaceCollider != null && parkingLotSpaceCollider == GetComponent<Collider2D>())
            {
                playerInParkingLot = true;

                // Start camera focus when entering the parking lot area
                if (cameraController != null && !cameraFocusActivated && parkingSpaceCollider != null)
                {
                    // Pass the parking SPACE transform and bounds to the camera
                    cameraController.StartParkingFocus(
                        parkingSpaceCollider.transform,
                        parkingSpaceCollider.bounds
                    );
                    cameraFocusActivated = true;

                    if (showDebugInfo)
                    {
                        Debug.Log($"Car entered parking LOT - camera focusing on parking SPACE at {parkingSpaceCollider.bounds.center} size {parkingSpaceCollider.bounds.size}");
                    }
                }
            }

            if (showDebugInfo)
            {
                Debug.Log($"Car entered trigger on {gameObject.name}");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the colliding object is the car
        if (other.CompareTag("Player") || other.GetComponent<CarController2D>() != null)
        {
            // This is the parking lot space trigger
            if (parkingLotSpaceCollider != null && parkingLotSpaceCollider == GetComponent<Collider2D>())
            {
                playerInParkingLot = false;

                // Stop camera focus when leaving the parking lot area
                if (cameraController != null && cameraFocusActivated)
                {
                    cameraController.StopParkingFocus();
                    cameraFocusActivated = false;

                    if (showDebugInfo)
                    {
                        Debug.Log("Car exited parking LOT, camera returning to normal");
                    }
                }

                // Also reset parking space status
                playerInParkingSpace = false;
                timeInZone = 0f;
            }

            if (showDebugInfo)
            {
                Debug.Log($"Car exited trigger on {gameObject.name}");
            }
        }
    }

    // Visualize the parking zones in the editor
    private void OnDrawGizmos()
    {
        // Draw parking lot space (camera trigger) in cyan
        if (parkingLotSpaceCollider != null)
        {
            Gizmos.color = playerInParkingLot ? Color.yellow : Color.cyan;
            Gizmos.DrawWireCube(parkingLotSpaceCollider.bounds.center, parkingLotSpaceCollider.bounds.size);
        }

        // Draw parking space (win condition) in green/yellow/red
        if (parkingSpaceCollider != null)
        {
            Gizmos.color = parkingCompleted ? Color.green : (playerInParkingSpace ? Color.yellow : Color.red);
            Gizmos.DrawWireCube(parkingSpaceCollider.bounds.center, parkingSpaceCollider.bounds.size);
        }

        // Draw car bounds if available
        if (carCollider != null && !parkingCompleted && Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Bounds carBounds = carCollider.bounds;
            Gizmos.DrawWireCube(carBounds.center, carBounds.size);
        }
    }
}