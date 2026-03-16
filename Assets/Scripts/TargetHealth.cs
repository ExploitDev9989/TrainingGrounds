using UnityEngine;

public class TargetHealth : MonoBehaviour
{
    public int maxHealth = 25;
    // starting health of the target
    // if weapon damage is >= this value the target will die in one shot

    [Header("FX")]
    public GameObject hitFX;
    // visual effect that spawns every time the target is hit

    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip destroySound;
    // hitSound plays every time damage is taken
    // destroySound plays when the target dies


    private int currentHealth;      // tracks the target's current remaining health
    private AudioSource audioSource; // audio source used to play sounds
    private bool isDying;
    // prevents the death logic from triggering multiple times

    void Awake()
    {
        currentHealth = maxHealth;
        // initialize the target's health when the object is created

        // try to get an AudioSource already attached to the object
        audioSource = GetComponent<AudioSource>();

        // if there isn't one, add one automatically
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        // ensures this object always has a way to play sounds

        // set some basic audio settings for world objects
        audioSource.playOnAwake = false; // prevents sound from playing automatically
        audioSource.loop = false;        // makes sure sounds do not repeat
        audioSource.spatialBlend = 1f;   // makes the sound 3D (volume changes with distance)

        // if spatialBlend was 0 it would act like UI audio and play at full volume everywhere
    }

    public void TakeDamage(int amount)
    {
        // if the object is already dying we stop here
        if (isDying) return;

        // subtract the incoming damage from the current health
        currentHealth -= amount;

        // spawn hit effect if one is assigned
        if (hitFX != null)
            Instantiate(hitFX, transform.position, Quaternion.identity);
        // the effect appears at the object's position

        // play hit sound if it exists
        if (hitSound != null)
            audioSource.PlayOneShot(hitSound);
        // PlayOneShot allows multiple sounds to overlap if shots happen quickly

        // check if the target's health has reached zero
        if (currentHealth <= 0)
        {
            isDying = true;
            // mark the object as dying so this block doesn't run again

            // play destruction sound if assigned
            if (destroySound != null)
                audioSource.PlayOneShot(destroySound);

            // destroy the target object shortly after death
            Destroy(gameObject, 0.05f);
            // small delay allows the destroy sound to begin playing
        }
    }
}