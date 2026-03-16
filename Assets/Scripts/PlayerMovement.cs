using UnityEngine;


// This makes sure the player always has a CharacterController component
// CharacterController handles collisions and movement with the environment
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    //class variables. public so user can change settings ingame
    [Header("Movement")]
    public float walkSpeed = 5f;       
    public float sprintSpeed = 9f;     
    public float gravity = -23f;       // force applieed every frame
    public float jumpHeight = 1f;       

    [Header("Sprint")]
    public KeyCode sprintKey = KeyCode.LeftShift; 
    public float sprintFOV = 63f;       
    public float normalFOV = 53f;      
    public float fovSmooth = 8f;       
    private float currentSpeed;        
    private bool isSprinting;            


    [Header("Mouse Look")]
    public Transform cameraTransform;  // used to rotate the camera up/down
    public Camera playerCam;           // reference to the player camera (used for FOV)
    public float mouseSensitivity = 2f;// how sensitive mouse movement is
    public float maxLookAngle = 85f;   // prevents looking too far up or down
    private CharacterController controller; // reference to Unity's CharacterController
    private Vector3 velocity;          // used for gravity and jumping movement
    private float xRotation = 0f;      // keeps track of vertical camera rotation

    void Awake()
    {
        // get the CharacterController component attached to the player
        controller = GetComponent<CharacterController>();

        // if the camera wasn't assigned in the inspector, find it automatically
        if (playerCam == null)
            playerCam = GetComponentInChildren<Camera>();

        // set cameraTransform if it wasn't manually assigned
        if (cameraTransform == null && playerCam != null)
            cameraTransform = playerCam.transform;

        // set the camera's starting field of view
        if (playerCam != null)
            playerCam.fieldOfView = normalFOV;

        // lock the mouse cursor so the player can look around like an FPS game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // handles mouse movement and camera rotation
        HandleMouseLook();

        // check if sprint key is pressed AND player is actually moving
        isSprinting = Input.GetKey(sprintKey) &&
                      (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f);

        // choose movement speed based on sprint state
        currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // handle movement and gravity
        HandleMovement();

        // update camera FOV when sprinting
        HandleFOV();
    }

    void HandleFOV()
    {
        // safety check so the game doesn't error if camera is gone
        if (playerCam == null)
            Debug.LogError("Playercam not assigned!");
        return;

        // choose which FOV we want depending on sprint state
        float targetFOV = isSprinting ? sprintFOV : normalFOV;

        // smoothly transition between FOV values
        playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, targetFOV, Time.deltaTime * fovSmooth);
    }

    void HandleMouseLook()
    {
        // read mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // rotate the player left/right
        transform.Rotate(Vector3.up * mouseX);

        // control vertical camera rotation
        xRotation -= mouseY;

        // prevent looking too far up or down
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        // apply the vertical rotation to the camera
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        
        float x = Input.GetAxis("Horizontal"); // A/D or left/right
        float z = Input.GetAxis("Vertical");   // W/S or forward/back

        // calculate movement direction relative to player orientation
        Vector3 move = (transform.right * x + transform.forward * z) * currentSpeed;

        // move player using CharacterController
        controller.Move(move * Time.deltaTime);

        // if player is grounded and falling, keep them snapped to ground
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // jump if player presses jump and is on the ground
        if (controller.isGrounded && Input.GetButtonDown("Jump"))
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // apply gravity
        velocity.y += gravity * Time.deltaTime;

        // apply vertical movement (gravity + jumping)
        controller.Move(velocity * Time.deltaTime);
    }
}