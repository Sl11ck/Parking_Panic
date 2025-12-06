using UnityEngine;

public class CarEngineSFXPlayer : MonoBehaviour
{
    [Header("Dependencies")]
    public CarController2D targetCar;
    public AudioClip carEngineSFX;

    [Header("Settings")]
    [SerializeField] private float minPitch = 0.8f; 
    [SerializeField] private float maxPitch = 2.0f;
    
    // Time in seconds between sounds at 0 RPM (Idle)
    [SerializeField] private float intervalAtIdle = 0.2f; 

    // Time in seconds between sounds at 1 RPM (Redline)
    [SerializeField] private float intervalAtMax = 0.05f;

    // Randomness to apply to pitch and timing (e.g., 0.01 or 0.02)
    [SerializeField] private float varianceAmount = 0.01f;

    // Internal State
    private float previous_maxSpeed;
    private float previous_update_maxSpeed;
    private float rpm;
    private float sfxTimer = 0f;

    void Start()
    {
        previous_maxSpeed = 0;
        // Initialize this to avoid immediate glitches on first frame
        if (targetCar != null) previous_update_maxSpeed = targetCar.maxSpeed;
    }

    void Update()
    {
        if (targetCar == null) return;

        // 1. Read raw car values
        float current_speed = targetCar.currentSpeed;
        float current_maxSpeed = targetCar.maxSpeed;

        // 2. Handle Gear/Max Speed Changes
        // We use else-if here to ensure we don't reset to 0 and then immediately 
        // overwrite it with the old speed in the same frame.
        if (current_maxSpeed < previous_update_maxSpeed)
        {
            // Downshift detected: Reset floor to 0 so math doesn't invert
            previous_maxSpeed = 0f; 
        }
        else if (current_maxSpeed > previous_update_maxSpeed)
        {
            // Upshift detected: The old max becomes the new floor
            previous_maxSpeed = previous_update_maxSpeed;
        }

        // Update the tracker for the next frame
        previous_update_maxSpeed = current_maxSpeed;

        // 3. Calculate RPM (0.0 to 1.0)
        // InverseLerp handles the math: (current - min) / (max - min)
        // It also handles Clamping automatically (result is never < 0 or > 1)
        rpm = Mathf.InverseLerp(previous_maxSpeed, current_maxSpeed, current_speed);

        // 4. Calculate Base Values
        float baseInterval = Mathf.Lerp(intervalAtIdle, intervalAtMax, rpm);
        float basePitch = Mathf.Lerp(minPitch, maxPitch, rpm);

        // 5. Add Variance (Randomness)
        float randomInterval = baseInterval + Random.Range(-varianceAmount, varianceAmount);
        float randomPitch = basePitch + Random.Range(-varianceAmount, varianceAmount);

        // Safety check to prevent interval from becoming 0 or negative
        if (randomInterval < 0.01f) randomInterval = 0.01f;

        // 6. Timer Logic
        sfxTimer += Time.deltaTime;

        if (sfxTimer >= randomInterval)
        {
            // Play sound with randomized pitch
            SFXManager.instance.PlaySFXClip(carEngineSFX, transform, 0.2f, randomPitch);
            
            sfxTimer = 0f; 
        }
    }
}
