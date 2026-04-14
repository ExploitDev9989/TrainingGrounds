using System;
using System.Collections;
using UnityEngine;

public class TargetHealth : MonoBehaviour, IDamageable
{
    public int maxHealth = 25;

    [Header("FX")]
    public GameObject hitFX;

    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip destroySound;

    // fires before the object is destroyed — HitMarkerUI can subscribe to this
    public event Action OnKilled;

    private int currentHealth;
    private AudioSource audioSource;
    private bool isDying;

    void Awake()
    {
        currentHealth = maxHealth;

        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
    }

    // FIX: returns true if this hit killed the target (WeaponShoot uses this for kill markers)
    public bool TakeDamage(int amount)
    {
        if (isDying) return false;

        currentHealth -= amount;

        if (hitFX != null)
            Instantiate(hitFX, transform.position, Quaternion.identity);

        if (hitSound != null)
            audioSource.PlayOneShot(hitSound);

        if (currentHealth <= 0)
        {
            isDying = true;
            OnKilled?.Invoke();
            StartCoroutine(Die());
            return true; // confirmed kill
        }

        return false;
    }

    IEnumerator Die()
    {
        // FIX: disable collider + renderer immediately so no more hits register
        // and the old code's 0.05f delay doesn't cut off the death sound
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;
        foreach (var rend in GetComponentsInChildren<Renderer>())
            rend.enabled = false;

        if (destroySound != null)
        {
            audioSource.PlayOneShot(destroySound);
            yield return new WaitForSeconds(destroySound.length);
        }
        else
        {
            yield return null;
        }

        Destroy(gameObject);
    }
}