using UnityEditor;
using UnityEngine;

public class CarController2D : MonoBehaviour
{
    public float steeringForce = 90f;       // degrees per sec steering speed
    public float maxSteerAngle = 75f;       // clamp steering angle
    public float acceleration = 25f;        // how fast speed changes
    public float maxSpeed = 80f;            // max car speed

    public float drag = 0.8f;
    private float speed = 0f;
    private float latentRotation;

    private Vector2 moveInput;

    private InputSystem_Actions _inputActions;
    private Rigidbody2D _rb;

    void Awake()
    {
        _inputActions = new InputSystem_Actions();
        TryGetComponent(out _rb);
    }

    private void OnEnable()
    {
        _inputActions.Enable();

        _inputActions.Player.Move.performed += 
            ctx => moveInput = ctx.ReadValue<Vector2>();

        _inputActions.Player.Move.canceled += 
            ctx => moveInput = Vector2.zero;
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    void Update()
    {
        float strInput = -moveInput.x;   // A/D
        float accInput = moveInput.y;   // W/S

        // Steering
        latentRotation += strInput * steeringForce * Time.deltaTime;
        latentRotation = Mathf.Clamp(latentRotation, -maxSteerAngle, maxSteerAngle);

        // Acceleration / braking
        speed += accInput * acceleration * Time.deltaTime;
        speed = Mathf.Clamp(speed, -maxSpeed, maxSpeed);
        speed -= speed * drag * Time.deltaTime;
    }

    void FixedUpdate()
    {
        // Apply steering to rotation
        _rb.MoveRotation(_rb.rotation + latentRotation * Time.deltaTime * _rb.linearVelocity.magnitude);

        // Set forward velocity directly (clean top-down handling)
        _rb.linearVelocity = transform.up * speed;
    }
}