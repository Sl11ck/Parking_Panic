using Unity.Mathematics;
using UnityEngine;

public class GearShifting : MonoBehaviour
{

    public int maxGear = 5;

    private int currentGear = 1;

    public float maxSpeedIncreasePerGear = 20f;
    public float gearPowerLoss = 1f;

    private float initialSteeringForce;
    private float initialMaxSpeed;
    private float initialAcceleration;
    private bool isGearingUp = false;
    private bool isGearingDown = false;

    private bool gearLock = false;
    private InputSystem_Actions _inputActions;
    private CarController2D _carController;

    void Awake()
    {
        _inputActions = new InputSystem_Actions();
        TryGetComponent(out _carController);
    }

    void Start()
    {
        initialSteeringForce = _carController.steeringForce;
        initialMaxSpeed = _carController.maxSpeed;
        initialAcceleration = _carController.acceleration;
    }

    private void OnEnable()
    {
        _inputActions.Enable();

        _inputActions.Player.GearUp.performed += 
            ctx => isGearingUp = true;

        _inputActions.Player.GearUp.canceled += 
            ctx => isGearingUp = false;

        _inputActions.Player.GearDown.performed += 
            ctx => isGearingDown = true;

        _inputActions.Player.GearDown.canceled += 
            ctx => isGearingDown = false;
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gearLock)
        {
            if (isGearingUp)
            {
                currentGear += 1;
                currentGear = Mathf.Clamp(currentGear, 1, maxGear);
                gearLock = true;
                SetCarGear();
            } else if (isGearingDown)
            {
                currentGear -= 1;
                currentGear = Mathf.Clamp(currentGear, 1, maxGear);
                gearLock = true;
                SetCarGear();
            }
        }

        if (gearLock)
        {
            if(!isGearingUp && !isGearingDown)
            {
                gearLock = false;
            }
        }
    }

    void SetCarGear()
    {
        _carController.maxSpeed = initialMaxSpeed + maxSpeedIncreasePerGear * currentGear;
        _carController.acceleration = initialAcceleration / (currentGear * gearPowerLoss);
        _carController.steeringForce = initialSteeringForce / (currentGear * gearPowerLoss);
    }
}
