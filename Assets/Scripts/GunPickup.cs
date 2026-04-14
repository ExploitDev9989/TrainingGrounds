using UnityEngine;
using TMPro;

public class GunPickup : MonoBehaviour
{
    public TMP_Text promptText;
    public GameObject weaponPrefabToEquip;

    // set this if you want the world pickup mesh to disappear after equipping
    // leave null to just disable the trigger (e.g. if the gun floats visually until picked up)
    public GameObject visualToHide;

    private bool playerInRange;
    private PlayerWeaponController playerWeaponController;

    void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;
        if (playerWeaponController == null || weaponPrefabToEquip == null) return;

        playerWeaponController.EquipWeapon(weaponPrefabToEquip);

        // FIX: disable after pickup so the player can't re-equip infinitely
        Pickup();
    }

    void Pickup()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);

        // hide visual if assigned, otherwise disable the whole object
        if (visualToHide != null)
            visualToHide.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        // try direct component first, then parent, then scene-wide fallback
        playerWeaponController =
            other.GetComponent<PlayerWeaponController>()
            ?? other.GetComponentInParent<PlayerWeaponController>()
            ?? FindFirstObjectByType<PlayerWeaponController>();

        if (promptText != null)
        {
            promptText.text = "Press E to pick up";
            promptText.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        playerWeaponController = null;

        if (promptText != null) promptText.gameObject.SetActive(false);
    }
}