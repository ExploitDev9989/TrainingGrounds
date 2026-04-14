using UnityEngine;

// Attach to the same GameObject as WeaponADS, or to the weapon socket parent.
// Adds three layers of feel on top of the ADS transitions:
//   1. Rotational sway  — weapon tilts opposite to mouse movement
//   2. Movement bob     — weapon bounces while walking/sprinting
//   3. Idle breathing   — subtle float while standing still
public class WeaponSway : MonoBehaviour
{
    [Header("Refs")]
    public Transform      weaponSocket;   // the transform to apply sway to (usually WeaponSocket)
    public PlayerMovement playerMovement; // auto-found if left null

    [Header("Mouse Sway")]
    public float swayAmount    = 0.02f;  // how far the weapon shifts position
    public float maxSway       = 0.06f;
    public float swaySmooth    = 8f;

    [Header("Rotational Sway")]
    public float rotSway       = 4f;     // degrees of tilt on mouse move
    public float rotSmooth     = 8f;

    [Header("Movement Bob")]
    public float bobFrequency  = 8f;
    public float bobX          = 0.005f; // side-to-side
    public float bobY          = 0.008f; // up-down
    public float sprintBobMult = 1.5f;   // bob is stronger when sprinting
    public float bobSmooth     = 10f;

    [Header("Idle Breathing")]
    public float breathX       = 0.003f;
    public float breathY       = 0.002f;
    public float breathSpeed   = 0.8f;

    // ── Private ──────────────────────────────────────────────────────────────
    private Vector3    initialLocalPos;
    private Quaternion initialLocalRot;
    private float      bobTimer;
    private Vector3    currentBob;
    private Vector3    bobRef;

    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (weaponSocket    == null) weaponSocket    = transform;
        if (playerMovement  == null) playerMovement  = FindFirstObjectByType<PlayerMovement>();
    }

    void Start()
    {
        // capture starting pose so we layer offsets on top without drifting
        initialLocalPos = weaponSocket.localPosition;
        initialLocalRot = weaponSocket.localRotation;
    }

    void LateUpdate()
    {
        // LateUpdate runs after WeaponADS moves the socket — we add sway on top of that
        UpdateBob();
        UpdateSway();
    }

    void UpdateBob()
    {
        bool moving   = playerMovement != null && playerMovement.MoveVelocity.magnitude > 0.4f;
        bool grounded = playerMovement == null || playerMovement.IsGrounded;

        if (moving && grounded)
        {
            float mult = playerMovement != null && playerMovement.IsSprinting ? sprintBobMult : 1f;
            bobTimer  += Time.deltaTime * bobFrequency * mult;

            float tx = Mathf.Sin(bobTimer)               * bobX;
            float ty = Mathf.Abs(Mathf.Sin(bobTimer))    * bobY; // abs gives a bounce (always up)
            currentBob = Vector3.SmoothDamp(currentBob, new Vector3(tx, ty, 0f), ref bobRef, 1f / bobSmooth);
        }
        else
        {
            // idle breathing — very subtle so it doesn't feel sick
            float bx = Mathf.Sin(Time.time * breathSpeed)        * breathX;
            float by = Mathf.Sin(Time.time * breathSpeed * 0.7f) * breathY;
            currentBob = Vector3.SmoothDamp(currentBob, new Vector3(bx, by, 0f), ref bobRef, 1f / bobSmooth);
        }
    }

    void UpdateSway()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // positional sway: weapon drifts slightly opposite to look direction
        Vector3 targetPos = new Vector3(
            Mathf.Clamp(-mouseX * swayAmount, -maxSway, maxSway),
            Mathf.Clamp(-mouseY * swayAmount, -maxSway, maxSway),
            0f
        );

        // rotational sway: weapon tilts with mouse (z gives nice roll)
        Quaternion targetRot = Quaternion.Euler(
             mouseY * rotSway,
             mouseX * rotSway,
             mouseX * rotSway * 1.5f
        );

        weaponSocket.localPosition = Vector3.Lerp(
            weaponSocket.localPosition,
            initialLocalPos + currentBob + targetPos,
            Time.deltaTime * swaySmooth
        );

        weaponSocket.localRotation = Quaternion.Slerp(
            weaponSocket.localRotation,
            initialLocalRot * targetRot,
            Time.deltaTime * rotSmooth
        );
    }
}
