using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(DemoHealth))]
public class SimpleDemoPlayerController : MonoBehaviour
{
    public static Vector3 LastNoisePosition { get; private set; }
    public static float LastNoiseTime { get; private set; } = -999f;
    public static float LastNoiseRadius { get; private set; }

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4.5f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("Noise")]
    [SerializeField] private KeyCode noiseKey = KeyCode.Y;
    [SerializeField] private float noiseRadius = 14f;
    [SerializeField] private float noiseMemorySeconds = 3f;

    [Header("Attack")]
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;
    [SerializeField] private float attackRange = 2.2f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private LayerMask attackMask = ~0;

    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform cameraPivot;

    private CharacterController controller;
    private DemoHealth health;
    private float verticalVelocity;
    private float cameraPitch;
    private float nextAttackTime;

    public float NoiseMemorySeconds => noiseMemorySeconds;
    public bool HasRecentNoise => Time.time - LastNoiseTime <= noiseMemorySeconds;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<DemoHealth>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (cameraPivot == null && playerCamera != null)
            cameraPivot = playerCamera.transform;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (health != null && health.IsDead)
            return;

        HandleLook();
        HandleMovement();
        HandleNoise();
        HandleAttack();
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        if (cameraPivot != null)
            cameraPivot.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        move = Vector3.ClampMagnitude(move, 1f);

        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * speed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleNoise()
    {
        if (!Input.GetKeyDown(noiseKey))
            return;

        LastNoisePosition = transform.position;
        LastNoiseTime = Time.time;
        LastNoiseRadius = noiseRadius;

        Debug.Log($"[PlayerNoise] Noise emitted at {LastNoisePosition} with radius {noiseRadius}");
    }

    private void HandleAttack()
    {
        if (!Input.GetKeyDown(attackKey))
            return;

        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackCooldown;

        if (playerCamera == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, attackRange, attackMask, QueryTriggerInteraction.Ignore))
        {
            DemoHealth targetHealth = hit.collider.GetComponentInParent<DemoHealth>();

            if (targetHealth != null && targetHealth != health)
            {
                targetHealth.Damage(attackDamage);
                Debug.Log($"[PlayerAttack] Hit {targetHealth.gameObject.name} for {attackDamage}");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Time.time - LastNoiseTime <= noiseMemorySeconds)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(LastNoisePosition, LastNoiseRadius);
        }
    }
}
