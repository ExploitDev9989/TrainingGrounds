using UnityEngine;
using UnityEngine.UI;

// ── Setup in Unity ────────────────────────────────────────────────────────────
// 1. Create a Canvas (Screen Space - Overlay).
// 2. Add an empty child "Crosshair" and attach this script.
// 3. Inside "Crosshair" add 5 UI Image children:
//      LineTop, LineBottom, LineLeft, LineRight, Dot
//    Each line should be a thin white rectangle (~2x10px).
//    Dot is a small square (~3x3px).
// 4. Assign those 5 RectTransforms to the fields below.
// 5. WeaponShoot and PlayerMovement are auto-found; you can also assign them manually.
// ─────────────────────────────────────────────────────────────────────────────
public class CrosshairUI : MonoBehaviour
{
    [Header("Lines — assign 4 Image children + optional dot")]
    public RectTransform lineTop;
    public RectTransform lineBottom;
    public RectTransform lineLeft;
    public RectTransform lineRight;
    public RectTransform dot; // hides when moving

    [Header("Spread")]
    public float baseSpread      = 4f;   // resting gap (px) from center
    public float maxSpread       = 45f;
    public float moveSpread      = 22f;  // added when walking
    public float sprintSpread    = 36f;  // added when sprinting
    public float recoilSpread    = 28f;  // added per shot
    public float spreadRecovery  = 9f;   // how fast it returns to base

    [Header("ADS")]
    // crosshair fades out when aiming — feels cleaner than just hiding
    public float adsFadeSpeed = 12f;

    [Header("Refs — auto-found if blank")]
    public WeaponShoot    weaponShoot;
    public PlayerMovement playerMovement;

    // ── Private ──────────────────────────────────────────────────────────────
    private float currentSpread;
    private float alpha = 1f;

    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        if (weaponShoot    == null) weaponShoot    = FindFirstObjectByType<WeaponShoot>();
        if (playerMovement == null) playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (weaponShoot != null)
            weaponShoot.OnShoot += HandleShoot;

        currentSpread = baseSpread;
    }

    void OnDestroy()
    {
        if (weaponShoot != null)
            weaponShoot.OnShoot -= HandleShoot;
    }

    void HandleShoot()
    {
        currentSpread = Mathf.Min(currentSpread + recoilSpread, maxSpread);
    }

    void Update()
    {
        bool isADS = Input.GetMouseButton(1);

        // fade crosshair out on ADS, in on hip
        float targetAlpha = isADS ? 0f : 1f;
        alpha = Mathf.Lerp(alpha, targetAlpha, Time.deltaTime * adsFadeSpeed);
        SetAlpha(alpha);

        if (!isADS) UpdateSpread();
        PositionLines();
    }

    void UpdateSpread()
    {
        float target = baseSpread;

        if (playerMovement != null)
        {
            float speed = playerMovement.MoveVelocity.magnitude;
            if (playerMovement.IsSprinting)
                target += sprintSpread;
            else if (speed > 0.3f)
                target += Mathf.Lerp(0f, moveSpread, speed / 5f);
        }

        // recoil decays toward target each frame
        currentSpread = Mathf.Lerp(currentSpread, target, Time.deltaTime * spreadRecovery);
        currentSpread = Mathf.Max(currentSpread, baseSpread);
    }

    void PositionLines()
    {
        float s = currentSpread;
        if (lineTop    != null) lineTop.anchoredPosition    = new Vector2(0,  s);
        if (lineBottom != null) lineBottom.anchoredPosition = new Vector2(0, -s);
        if (lineLeft   != null) lineLeft.anchoredPosition   = new Vector2(-s, 0);
        if (lineRight  != null) lineRight.anchoredPosition  = new Vector2( s, 0);

        // hide center dot when moving — cleaner look
        if (dot != null)
        {
            bool moving = playerMovement != null && playerMovement.MoveVelocity.magnitude > 0.3f;
            dot.gameObject.SetActive(!moving);
        }
    }

    void SetAlpha(float a)
    {
        SetImageAlpha(lineTop,    a);
        SetImageAlpha(lineBottom, a);
        SetImageAlpha(lineLeft,   a);
        SetImageAlpha(lineRight,  a);
        SetImageAlpha(dot,        a);
    }

    void SetImageAlpha(RectTransform rt, float a)
    {
        if (rt == null) return;
        var img = rt.GetComponent<Image>();
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}
