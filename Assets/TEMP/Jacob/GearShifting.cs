using UnityEngine;

public class GearShifting : MonoBehaviour
{
    public int maxGear = 5;
    public float maxSpeedIncreasePerGear = 20f;
    public float gearPowerLoss = 1f;

    public float minReqStrictness = 0.8f;

    private int currentGear = 1;
    private float initialSteeringForce;
    private float initialMaxSpeed;
    private float initialAcceleration;

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

        _inputActions.Player.GearUp.performed += ctx => ShiftUp();
        _inputActions.Player.GearDown.performed += ctx => ShiftDown();
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    void ShiftUp()
    {
        currentGear = Mathf.Clamp(currentGear + 1, 1, maxGear);
        SetCarGear();
    }

    void ShiftDown()
    {
        currentGear = Mathf.Clamp(currentGear - 1, 1, maxGear);
        SetCarGear();
    }

    void SetCarGear()
    {
        _carController.maxSpeed = initialMaxSpeed + maxSpeedIncreasePerGear * (currentGear - 1);
        _carController.acceleration = initialAcceleration / Mathf.Pow(currentGear, gearPowerLoss);
        _carController.steeringForce = initialSteeringForce / Mathf.Pow(currentGear, gearPowerLoss);
        _carController.minimumSpeedReq = (initialMaxSpeed + maxSpeedIncreasePerGear * (currentGear - 2)) * minReqStrictness;
    }
}