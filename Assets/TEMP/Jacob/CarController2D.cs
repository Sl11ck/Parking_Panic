using UnityEditor;
using UnityEngine;

public class CarController2D : MonoBehaviour
{
    public float steeringForce = 90f;       // degrees per sec steering speed
    public float maxSteerAngle = 75f;       // clamp steering angle
    public float acceleration = 25f;        // how fast speed changes
    public float maxSpeed = 2f;            // max car speed

    public float minimumSpeedReq = 2f;

    public float steeringDrag = 0.2f;

    public float drag = 0.8f;
    private float speed = 0f;
    private float latentRotation;

    private int paintChips = 0;
    public Sprite[] carSpritesPaintChip;

    private Vector2 moveInput;

    private InputSystem_Actions _inputActions;
    private Rigidbody2D _rb;
    public float currentSpeed => speed;

    [SerializeField] GameObject wheel1, wheel2;

    private SignManager signManager;

    public float collisionCooldown = 1f;
    private float colCooldown;

    void Awake()
    {
        _inputActions = new InputSystem_Actions();
        TryGetComponent(out _rb);
    }

    void Start()
    {
        signManager = GameObject.FindGameObjectWithTag("SignManager").GetComponent<SignManager>();
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
        latentRotation -= latentRotation * _rb.linearVelocity.magnitude * steeringDrag * Time.deltaTime;

        wheel1.transform.localRotation = Quaternion.Euler(
            transform.localRotation.eulerAngles.x, 
            transform.localRotation.eulerAngles.y,
            latentRotation
        );

        wheel2.transform.localRotation = Quaternion.Euler(
            transform.localRotation.eulerAngles.x, 
            transform.localRotation.eulerAngles.y,
            latentRotation
        );

        // Acceleration / braking
        if (speed >= minimumSpeedReq)
        {
            speed += accInput * acceleration * Time.deltaTime;
            speed = Mathf.Clamp(speed, -maxSpeed, maxSpeed);
        }
        speed -= speed * drag * Time.deltaTime;

        colCooldown -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        // Apply steering to rotation
        float steeringSign = Mathf.Sign(speed); // +1 forward, -1 reverse

        _rb.MoveRotation(
            _rb.rotation 
            + latentRotation * steeringSign * Time.deltaTime * _rb.linearVelocity.magnitude
        );

        // Set forward velocity directly (clean top-down handling)
        _rb.linearVelocity = transform.up * speed;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (colCooldown <= 0)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("MinorCollisions"))
            {
                signManager.RegisterMistake();
                paintChips++;
                paintChips = Mathf.Clamp(paintChips, 0, carSpritesPaintChip.Length - 1);
                GetComponent<SpriteRenderer>().sprite = carSpritesPaintChip[paintChips];
                colCooldown = collisionCooldown;
            }
        }
    }
}