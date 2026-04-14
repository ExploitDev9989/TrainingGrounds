using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ── Setup in Unity ────────────────────────────────────────────────────────────
// 1. Inside your Canvas create an empty "HitMarker" and attach this script.
// 2. Add 4 Image children: HMTop, HMBottom, HMLeft, HMRight.
//    Rotate each 45° so they form an X shape (hit marker style).
//    Make each about 2x12px, white.
// 3. Put all 4 under a parent with a CanvasGroup component — assign that parent
//    to hitMarkerParent.
// 4. For the kill feed, create a vertical LayoutGroup panel in the top-right
//    corner and assign it to killFeedContainer. Create a simple TMP_Text prefab
//    and assign to killFeedEntryPrefab.
// 5. Assign hitMarkerLines to the 4 Images.
// 6. WeaponShoot is auto-found; you can also call SetWeapon() from PlayerWeaponController.
// ─────────────────────────────────────────────────────────────────────────────
public class HitMarkerUI : MonoBehaviour
{
    [Header("Hit Marker")]
    public CanvasGroup hitMarkerParent;      // CanvasGroup controls alpha of all 4 lines
    public Image[]     hitMarkerLines;       // the 4 diagonal Image lines
    public float       hitDuration  = 0.12f;
    public Color       hitColor     = Color.white;
    public Color       killColor    = new Color(1f, 0.25f, 0.25f, 1f); // red tint on kill

    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip killSound;

    [Header("Kill Feed")]
    public Transform  killFeedContainer;     // vertical layout group in top-right
    public GameObject killFeedEntryPrefab;   // prefab with a TMP_Text component
    public int        maxEntries    = 4;
    public float      entryDuration = 3.5f;
    public string     playerName    = "Player";
    public string     targetName    = "Target";

    // ── Private ──────────────────────────────────────────────────────────────
    private WeaponShoot  currentWeapon;
    private AudioSource  audioSource;
    private Coroutine    hitRoutine;
    private List<GameObject> feedEntries = new List<GameObject>();

    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (hitMarkerParent != null) hitMarkerParent.alpha = 0f;
    }

    void Start()
    {
        // auto-find if not set via PlayerWeaponController
        if (currentWeapon == null)
            SetWeapon(FindFirstObjectByType<WeaponShoot>());
    }

    public void SetWeapon(WeaponShoot newWeapon)
    {
        if (currentWeapon != null) currentWeapon.OnHit -= HandleHit;
        currentWeapon = newWeapon;
        if (currentWeapon != null) currentWeapon.OnHit += HandleHit;
    }

    void OnDestroy()
    {
        if (currentWeapon != null) currentWeapon.OnHit -= HandleHit;
    }

    void HandleHit(bool isKill)
    {
        // restart flash so rapid hits don't stack weirdly
        if (hitRoutine != null) StopCoroutine(hitRoutine);
        hitRoutine = StartCoroutine(FlashHitMarker(isKill));

        AudioClip clip = isKill ? killSound : hitSound;
        if (clip != null) audioSource.PlayOneShot(clip);

        if (isKill) AddKillFeedEntry();
    }

    IEnumerator FlashHitMarker(bool isKill)
    {
        Color c = isKill ? killColor : hitColor;
        foreach (var img in hitMarkerLines)
            if (img != null) img.color = c;

        // fade out over duration
        float elapsed = 0f;
        while (elapsed < hitDuration)
        {
            elapsed += Time.deltaTime;
            if (hitMarkerParent != null)
                hitMarkerParent.alpha = 1f - (elapsed / hitDuration);
            yield return null;
        }

        if (hitMarkerParent != null) hitMarkerParent.alpha = 0f;
    }

    void AddKillFeedEntry()
    {
        if (killFeedContainer == null || killFeedEntryPrefab == null) return;

        // remove oldest entry if at limit
        if (feedEntries.Count >= maxEntries)
        {
            var oldest = feedEntries[0];
            feedEntries.RemoveAt(0);
            if (oldest != null) Destroy(oldest);
        }

        GameObject entry = Instantiate(killFeedEntryPrefab, killFeedContainer);
        var tmp = entry.GetComponent<TMP_Text>();
        if (tmp != null)
            tmp.text = $"<b>{playerName}</b>  <color=#FF4444>✕</color>  {targetName}";

        feedEntries.Add(entry);
        StartCoroutine(RemoveEntry(entry, entryDuration));
    }

    IEnumerator RemoveEntry(GameObject entry, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (entry != null)
        {
            feedEntries.Remove(entry);
            Destroy(entry);
        }
    }
}
