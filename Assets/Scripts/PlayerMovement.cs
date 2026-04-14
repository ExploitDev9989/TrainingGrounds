using UnityEngine;

// MW2019-inspired player movement:
// smooth acceleration, strafe camera tilt, coyote jump, toggle crouch
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8.5f;
    public float crouchSpeed = 2.5f;
    public float acceleration = 14f; // how quickly speed ramps up — higher = snappier
    public float deceleration = 18f; // how quickly speed ramps down
    public float gravity = -23f;
    public float jumpHeight = 1.1f;

    [Header("Coyote Jump")]
    // lets the player jump for a short window after walking off a ledge
    public float coyoteTime = 0.12f;

    [Header("Sprint")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintFOV = 63f;
    public float normalFOV = 53f;
    public float fovSmooth = 8f;

    [Header("Crouch (toggle)")]
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float standHeight = 2.5f;
    public float crouchHeight = 1.1f;
    public float crouchSmooth = 10f;

    [Header("Mouse Look")]
    public Transform cameraTransform;
    public Camera playerCam;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 85f;

    [Header("Camera Strafe Tilt")]
    // camera rolls slightly when moving left/right — feels grounded like MW2019
    public float tiltAngle = 3f;
    public float tiltSmooth = 8f;

    // ── Private ──────────────────────────────────────────────────────────────
    private CharacterController cc;
    private Vector3 velocity;            // gravity / jump
    private float xRotation;           // vertical camera angle
    private float coyoteTimer;
    private bool isCrouching;

    // smooth movement uses SmoothDamp so speed changes feel physical
    private Vector3 moveVelocity;
    private Vector3 moveSmoothRef;

    // ── Public read-only ─────────────────────────────────────────────────────
    public bool IsSprinting { get; private set; }
    public bool IsCrouching => isCrouching;
    public bool IsGrounded => cc.isGrounded;
    public Vector3 MoveVelocity => moveVelocity; // WeaponSway reads this

    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        if (playerCam == null) playerCam = GetComponentInChildren<Camera>();
        if (cameraTransform == null && playerCam != null) cameraTransform = playerCam.transform;
        if (playerCam != null) playerCam.fieldOfView = normalFOV;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleCrouch();
        HandleFOV();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // yaw: rotate player body left/right
        transform.Rotate(Vector3.up * mouseX);

        // pitch: rotate camera up/down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        // strafe tilt: camera rolls when moving left/right
        float strafeInput = Input.GetAxis("Horizontal");
        float targetTilt = -strafeInput * tiltAngle;

        if (cameraTransform != null)
        {
            // unwrap z from Unity's 0-360 range to -180-180 before lerping
            float currentZ = cameraTransform.localEulerAngles.z;
            if (currentZ > 180f) currentZ -= 360f;
            float smoothZ = Mathf.Lerp(currentZ, targetTilt, Time.deltaTime * tiltSmooth);

            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, smoothZ);
        }
    }

    void HandleMovement()
    {
        bool grounded = cc.isGrounded;

        if (grounded)
        {
            coyoteTimer = coyoteTime;
            if (velocity.y < 0f) velocity.y = -2f; // keep character snapped to ground
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // sprint only when moving forward — not backwards or sideways (MW2019 behaviour)
        IsSprinting = Input.GetKey(sprintKey) && v > 0.1f && !isCrouching;

        float targetSpeed = isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed;
        Vector3 rawMove = (transform.right * h + transform.forward * v).normalized * targetSpeed;

        // accelerate faster than we decelerate — feels responsive but not jerky
        float smoothTime = rawMove.magnitude > moveVelocity.magnitude
            ? 1f / acceleration
            : 1f / deceleration;

        moveVelocity = Vector3.SmoothDamp(moveVelocity, rawMove, ref moveSmoothRef, smoothTime);
        cc.Move(moveVelocity * Time.deltaTime);

        // coyote jump — still lets you jump briefly after stepping off a ledge
        if (Input.GetButtonDown("Jump") && coyoteTimer > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            coyoteTimer = 0f;
        }

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }

    void HandleCrouch()
    {
        // toggle on keydown rather than hold — more comfortable for long sessions
        if (Input.GetKeyDown(crouchKey))
            isCrouching = !isCrouching;

        float targetHeight = isCrouching ? crouchHeight : standHeight;
        cc.height = Mathf.Lerp(cc.height, targetHeight, Time.deltaTime * crouchSmooth);

        // keep feet on the ground as height changes
        cc.center = new Vector3(0f, cc.height * 0.5f, 0f);
    }

    void HandleFOV()
    {
        if (playerCam == null) return;
        float target = IsSprinting ? sprintFOV : normalFOV;
        playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, target, Time.deltaTime * fovSmooth);
    }
}