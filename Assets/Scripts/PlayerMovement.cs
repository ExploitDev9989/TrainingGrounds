using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10.5f;
    public float acceleration = 14f;
    public float deceleration = 18f;
    public float gravity = -23f;
    public float jumpHeight = 1.1f;

    [Header("Coyote Jump")]
    public float coyoteTime = 0.12f;

    [Header("Sprint")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintFOV = 63f;
    public float normalFOV = 53f;
    public float fovSmooth = 8f;

    [Header("Mouse Look")]
    public Transform cameraTransform;
    public Camera playerCam;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 85f;

    [Header("Camera Strafe Tilt")]
    public float tiltAngle = 2f;
    public float tiltSmooth = 8f;

    [Header("Capsule")]
    public float standHeight = 1.8f;
    public float centerOffset = 0f;

    // ── Private ──────────────────────────────────────────────────────────────
    private CharacterController cc;
    private Vector3 velocity;
    private float xRotation;
    private float coyoteTimer;
    private Vector3 moveVelocity;
    private Vector3 moveSmoothRef;

    // ── Public read-only ─────────────────────────────────────────────────────
    public bool IsSprinting { get; private set; }
    public bool IsGrounded => cc.isGrounded;
    public Vector3 MoveVelocity => moveVelocity;

    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        if (playerCam == null) playerCam = GetComponentInChildren<Camera>();
        if (cameraTransform == null && playerCam != null) cameraTransform = playerCam.transform;
        if (playerCam != null) playerCam.fieldOfView = normalFOV;

        // set capsule size once at start
        cc.height = standHeight;
        cc.center = new Vector3(0f, standHeight * 0.5f + centerOffset, 0f);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleFOV();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        float strafeInput = Input.GetAxis("Horizontal");
        float targetTilt = -strafeInput * tiltAngle;

        if (cameraTransform != null)
        {
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
            if (velocity.y < 0f) velocity.y = -2f;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        IsSprinting = Input.GetKey(sprintKey) && v > 0.1f;

        float targetSpeed = IsSprinting ? sprintSpeed : walkSpeed;
        Vector3 rawMove = (transform.right * h + transform.forward * v).normalized * targetSpeed;

        float smoothTime = rawMove.magnitude > moveVelocity.magnitude
            ? 1f / acceleration
            : 1f / deceleration;

        moveVelocity = Vector3.SmoothDamp(moveVelocity, rawMove, ref moveSmoothRef, smoothTime);
        cc.Move(moveVelocity * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && coyoteTimer > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            coyoteTimer = 0f;
        }

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }

    void HandleFOV()
    {
        if (playerCam == null) return;
        float target = IsSprinting ? sprintFOV : normalFOV;
        playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, target, Time.deltaTime * fovSmooth);
    }
}
