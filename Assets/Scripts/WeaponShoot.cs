using System;
using System.Collections;
using UnityEngine;

public class WeaponShoot : MonoBehaviour
{
    [Header("Refs")]
    public Camera playerCam;
    public Transform firePoint;

    [Header("Viewmodel Poses")]
    public Transform hipPos;
    public Transform adsPos;

    [Header("Shooting")]
    public float range = 100f;
    public float fireRate = 10f;
    public int damage = 25;

    public enum FireMode { Semi, FullAuto }
    public FireMode fireMode = FireMode.Semi;

    [Header("Ammo")]
    public int magSize = 12;
    public int ammoInMag = 12;
    public int reserveAmmo = 60;
    public float reloadTime = 1.2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootClip;
    public AudioClip[] reloadClips;
    public AudioClip dryFireClip;

    [Header("FX")]
    public ParticleSystem muzzleFlash;
    public GameObject hitVfxPrefab;

    [Header("Raycast")]
    public LayerMask hitMask = ~0;

    [Header("Animator")]
    public Animator weaponAnimator;

    // ── Events ──────────────────────────────────────────────────────────────
    // CrosshairUI and HitMarkerUI subscribe to these
    public event Action<bool> OnHit;    // bool = isKill
    public event Action OnAmmoChanged;
    public event Action OnShoot;  // crosshair recoil

    // ── Public read-only state ───────────────────────────────────────────────
    public bool IsReloading => isReloading;
    public bool IsADS => Input.GetMouseButton(1);

    // ── Private ──────────────────────────────────────────────────────────────
    private float nextTimeToFire;
    private bool isReloading;

    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        enabled = false; // disabled until PlayerWeaponController equips this

        if (playerCam == null) playerCam = Camera.main;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (firePoint == null)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "ShootSource") { firePoint = t; break; }
            }
        }

        if (firePoint == null)
            Debug.LogWarning($"{name}: firePoint not set — assign ShootSource child.");
    }

    void OnEnable()
    {
        nextTimeToFire = Time.time;
        isReloading = false;
    }

    void Update()
    {
        if (isReloading) return;

        bool triggerDown = fireMode == FireMode.FullAuto
            ? Input.GetMouseButton(0)
            : Input.GetMouseButtonDown(0);

        // auto-reload when dry-firing with reserve ammo available
        if (triggerDown && ammoInMag <= 0 && reserveAmmo > 0)
        {
            StartCoroutine(Reload());
            return;
        }

        // manual reload
        if (Input.GetKeyDown(KeyCode.R) && ammoInMag < magSize && reserveAmmo > 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (triggerDown && Time.time >= nextTimeToFire)
        {
            if (ammoInMag <= 0)
            {
                audioSource?.PlayOneShot(dryFireClip);
                nextTimeToFire = Time.time + 0.3f; // short cooldown to avoid sound spam
                return;
            }

            nextTimeToFire = Time.time + 1f / Mathf.Max(0.01f, fireRate);
            ammoInMag--;
            OnAmmoChanged?.Invoke();
            Shoot();
        }
    }

    void Shoot()
    {
        audioSource?.PlayOneShot(shootClip);
        muzzleFlash?.Play();
        OnShoot?.Invoke(); // crosshair reacts to this

        if (weaponAnimator != null)
        {
            weaponAnimator.ResetTrigger("Reload");
            weaponAnimator.SetTrigger("Shoot");
        }

        if (firePoint == null)
        {
            Debug.LogWarning("No firePoint assigned!");
            return;
        }

        Ray ray = new Ray(firePoint.position, firePoint.forward);
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 0.2f);

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            // FIX: only call IDamageable — TargetHealth now implements it so no more double damage
            var dmg = hit.collider.GetComponentInParent<IDamageable>();
            bool killed = false;
            if (dmg != null)
                killed = dmg.TakeDamage(damage); // returns true if this shot killed the target

            OnHit?.Invoke(killed); // tells HitMarkerUI whether to show a kill marker

            if (hitVfxPrefab != null)
            {
                var fx = Instantiate(hitVfxPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(fx, 1.5f);
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

        // FIX: scale clip timings proportionally so any reloadTime value works safely
        float t = Mathf.Max(0.1f, reloadTime);
        float t1 = t * 0.29f;  // ~first third
        float t2 = t * 0.38f;  // ~middle
        float t3 = t - t1 - t2; // remainder (always positive)

        if (reloadClips.Length > 0) audioSource?.PlayOneShot(reloadClips[0]);
        yield return new WaitForSeconds(t1);

        if (reloadClips.Length > 1) audioSource?.PlayOneShot(reloadClips[1]);
        yield return new WaitForSeconds(t2);

        if (reloadClips.Length > 2) audioSource?.PlayOneShot(reloadClips[2]);
        yield return new WaitForSeconds(t3);

        int toLoad = Mathf.Min(magSize - ammoInMag, reserveAmmo);
        ammoInMag += toLoad;
        reserveAmmo -= toLoad;
        isReloading = false;
        OnAmmoChanged?.Invoke();
    }
}