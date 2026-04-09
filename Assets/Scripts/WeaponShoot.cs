using System.Collections;
using UnityEngine;

public class WeaponShoot : MonoBehaviour
{
    [Header("Refs")]
    public Camera playerCam;               // main camera used for aiming
    public Transform firePoint;            // where the shot/ray starts from (muzzle / ShootSource)

    [Header("Viewmodel Poses (per weapon)")]
    public Transform hipPos;               // weapon position when not aiming
    public Transform adsPos;               // weapon position when aiming down sights

    [Header("Shooting")]
    public float range = 100f;             // how far the shot can hit
    public float fireRate = 10f;           // shots per second
    public int damage = 25;                // damage per shot

    public enum FireMode { Semi, FullAuto } // fire mode options
    public FireMode fireMode = FireMode.Semi; // default is semi-auto

    [Header("Ammo")]
    public int magSize = 12;               // bullets per magazine
    public int ammoInMag = 12;             // current bullets in mag
    public int reserveAmmo = 60;           // extra bullets stored
    public float reloadTime = 1.2f;        // how long reload takes

    [Header("Audio")]
    public AudioSource audioSource;        // audio source on the weapon
    public AudioClip shootClip;            // sound for firing
    public AudioClip[] reloadClips;        // changed from single clip to array
    public AudioClip dryFireClip;          // sound when empty

    [Header("FX (optional)")]
    public ParticleSystem muzzleFlash;     // muzzle flash particle effect
    public GameObject hitVfxPrefab;        // impact effect prefab

    [Header("Raycast Filtering (IMPORTANT)")]
    public LayerMask hitMask = ~0;         // what the raycast can hit (exclude player/weapon in inspector)

    private float nextTimeToFire = 0f;     // used to limit fire rate
    private bool isReloading = false;      // blocks shooting while reloading

    [Header("Animator")]
    public Animator weaponAnimator;        // animator for shoot/reload animations

    void Awake()
    {
        // if camera wasn't set manually, grab the main camera
        if (playerCam == null)
            playerCam = Camera.main;

        // if audio source wasn't assigned, try to grab it from this object
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // FirePoint setup:
        // best option is assigning in Inspector, but this is a backup search
        if (firePoint == null)
        {
            Transform[] all = GetComponentsInChildren<Transform>(true);
            foreach (var t in all)
            {
                // look for a child named "ShootSource"
                if (t.name == "ShootSource")
                {
                    firePoint = t;
                    break;
                }
            }
        }

        // warning if firePoint still wasn't found
        if (firePoint == null)
            Debug.LogWarning($"{name}: firePoint is NOT set. Drag USP-S/ShootSource into WeaponShoot.firePoint.");
    }

    void OnEnable()
    {
        // reset timing so the gun can shoot immediately when enabled
        nextTimeToFire = Time.time;
        isReloading = false;
    }

    void Update()
    {
        // if we're reloading, don't allow any shooting input
        if (isReloading) return;

        // Reload: only reload if mag isn't full AND we have reserve ammo
        if (Input.GetKeyDown(KeyCode.R) && ammoInMag < magSize && reserveAmmo > 0)
        {
            StartCoroutine(Reload()); // coroutine lets us wait without freezing the game
            return;
        }

        // decide the correct input based on fire mode:
        // FullAuto = hold mouse button, Semi = click once
        bool trigger =
            fireMode == FireMode.FullAuto ? Input.GetMouseButton(0) :
            Input.GetMouseButtonDown(0);

        // fire if trigger is pressed AND enough time has passed based on fireRate
        if (trigger && Time.time >= nextTimeToFire)
        {
            // if no ammo, play dry fire sound and add a small delay
            if (ammoInMag <= 0)
            {
                audioSource?.PlayOneShot(dryFireClip);
                nextTimeToFire = Time.time + 0.15f; // small cooldown so it doesn't spam sound
                return;
            }

            // make sure fireRate can't be 0 (prevents divide by zero)
            float safeFireRate = Mathf.Max(0.01f, fireRate);

            // set the next time we are allowed to fire
            nextTimeToFire = Time.time + (1f / safeFireRate);

            // spend one bullet and shoot
            ammoInMag--;
            Shoot();
        }
    }

    void Shoot()
    {
        // play shooting sound and muzzle flash (if assigned)
        audioSource?.PlayOneShot(shootClip);
        muzzleFlash?.Play();

        // trigger shoot animation (and cancel reload trigger if needed)
        if (weaponAnimator != null)
        {
            weaponAnimator.ResetTrigger("Reload");
            weaponAnimator.SetTrigger("Shoot");
        }

        // safety check so we don't raycast with no fire point
        if (firePoint == null)
        {
            Debug.LogWarning("No firePoint assigned!");
            return;
        }

        // raycast forward from the gun muzzle direction
        Ray gunRay = new Ray(firePoint.position, firePoint.forward);
        Debug.DrawRay(gunRay.origin, gunRay.direction * range, Color.red, 0.2f); // for debugging in scene view

        // if ray hits something, apply damage + spawn impact effect
        if (Physics.Raycast(gunRay, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            // damage targets that use TargetHealth
            var th = hit.collider.GetComponentInParent<TargetHealth>();
            if (th != null) th.TakeDamage(damage);

            // damage anything that implements IDamageable (more reusable system)
            var dmg = hit.collider.GetComponentInParent<IDamageable>();
            if (dmg != null) dmg.TakeDamage(damage);

            // spawn hit effect at the hit point, rotated to match the surface normal
            if (hitVfxPrefab != null)
            {
                Quaternion rot = Quaternion.LookRotation(hit.normal);
                GameObject fx = Instantiate(hitVfxPrefab, hit.point, rot);
                Destroy(fx, 1.5f); // clean up effect after a short time
            }
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;

        if (weaponAnimator != null)
        {
            weaponAnimator.ResetTrigger("Shoot");
            weaponAnimator.SetTrigger("Reload");
        }

        if (reloadClips.Length > 0)
            audioSource?.PlayOneShot(reloadClips[0]);

        yield return new WaitForSeconds(0.35f);

        if (reloadClips.Length > 1)
            audioSource?.PlayOneShot(reloadClips[1]);

        yield return new WaitForSeconds(0.45f);

        if (reloadClips.Length > 2)
            audioSource?.PlayOneShot(reloadClips[2]);

        yield return new WaitForSeconds(reloadTime - 0.8f);

        int needed = magSize - ammoInMag;
        int toLoad = Mathf.Min(needed, reserveAmmo);

        ammoInMag += toLoad;
        reserveAmmo -= toLoad;

        isReloading = false;
    }
}