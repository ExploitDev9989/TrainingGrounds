using System.Collections.Generic;
using TMPro;
using UnityEngine;

// ── Setup ─────────────────────────────────────────────────────────────────────
// 1. Place this script on your button object in the scene (e.g. a panel or box).
// 2. Add a Collider to the button object and tick "Is Trigger".
// 3. Assign the targets you want to respawn in the "Targets" list in the inspector.
//    Each entry needs the original prefab and the spawn point transform.
// 4. Optionally assign a TMP_Text for the prompt ("Press E to reset targets").
// ─────────────────────────────────────────────────────────────────────────────

public class TargetResetButton : MonoBehaviour
{
    [System.Serializable]
    public class TargetEntry
    {
        public GameObject prefab;       // the target prefab to spawn
        public Transform  spawnPoint;   // where to spawn it
    }

    [Header("Targets")]
    public List<TargetEntry> targets = new List<TargetEntry>();

    [Header("Prompt")]
    public TMP_Text promptText;
    public string   promptMessage = "Press E to reset targets";

    // ── Private ──────────────────────────────────────────────────────────────
    private bool playerInRange;
    private List<GameObject> spawnedTargets = new List<GameObject>();

    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);

        // spawn all targets at the start so the range is ready to go immediately
        SpawnAll();
    }

    void Update()
    {
        if (!playerInRange) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        ResetTargets();
    }

    void ResetTargets()
    {
        // destroy any targets still alive
        foreach (var t in spawnedTargets)
            if (t != null) Destroy(t);

        spawnedTargets.Clear();
        SpawnAll();

        Debug.Log("Targets reset!");
    }

    void SpawnAll()
    {
        foreach (var entry in targets)
        {
            if (entry.prefab == null || entry.spawnPoint == null) continue;

            GameObject instance = Instantiate(
                entry.prefab,
                entry.spawnPoint.position,
                entry.spawnPoint.rotation
            );

            spawnedTargets.Add(instance);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (promptText != null)
        {
            promptText.text = promptMessage;
            promptText.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }
}
