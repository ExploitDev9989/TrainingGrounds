using UnityEngine;
using TMPro;

public class GunPickup : MonoBehaviour
{
    public TMP_Text promptText;
    public GameObject weaponPrefabToEquip;

    private bool playerInRange = false;
    private PlayerWeaponController playerWeaponController;

    void Start()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E pressed");

            if (playerWeaponController == null)
            {
                Debug.LogWarning("playerWeaponController is NULL");
                return;
            }

            if (weaponPrefabToEquip == null)
            {
                Debug.LogWarning("weaponPrefabToEquip is NULL");
                return;
            }

            playerWeaponController.EquipWeapon(weaponPrefabToEquip);
            Debug.Log("Equipped: " + weaponPrefabToEquip.name);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered trigger: " + other.name + " | tag = " + other.tag);

        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            playerWeaponController = other.GetComponent<PlayerWeaponController>();

            if (playerWeaponController == null)
                playerWeaponController = other.GetComponentInParent<PlayerWeaponController>();

            if (playerWeaponController == null)
                playerWeaponController = FindFirstObjectByType<PlayerWeaponController>();

            Debug.Log("Controller found? " + (playerWeaponController != null));

            if (promptText != null)
            {
                promptText.text = "Press E to equip gun";
                promptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerWeaponController = null;

            if (promptText != null)
                promptText.gameObject.SetActive(false);
        }
    }
}