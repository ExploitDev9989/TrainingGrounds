using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Refs")]
    public Transform weaponSocket;
    public Camera playerCam;
    public WeaponADS weaponADS;
    public AmmoUI ammoUI;

    private WeaponShoot currentWeaponShoot;

    void Awake()
    {
        if (weaponADS == null)
            weaponADS = GetComponentInChildren<WeaponADS>();

        if (playerCam == null)
            playerCam = GetComponentInChildren<Camera>();
    }

    void Start()
    {
        DisableAllWeaponShoots();
        SetupExistingWeapon();
    }

    void DisableAllWeaponShoots()
    {
        WeaponShoot[] allWeapons = FindObjectsByType<WeaponShoot>(FindObjectsSortMode.None);

        foreach (WeaponShoot weapon in allWeapons)
        {
            weapon.enabled = false;
        }
    }

    public void SetupExistingWeapon()
    {
        if (weaponSocket == null)
        {
            Debug.LogError("PlayerWeaponController: weaponSocket not assigned!");
            return;
        }

        currentWeaponShoot = weaponSocket.GetComponentInChildren<WeaponShoot>();

        if (currentWeaponShoot == null)
        {
            Debug.LogWarning("No WeaponShoot found under weaponSocket.");
            return;
        }

        DisableAllWeaponShoots();

        currentWeaponShoot.playerCam = playerCam;
        currentWeaponShoot.enabled = true;

        if (weaponADS != null &&
            currentWeaponShoot.hipPos != null &&
            currentWeaponShoot.adsPos != null)
        {
            weaponADS.SetPoses(currentWeaponShoot.hipPos, currentWeaponShoot.dsPos, true);
        }

        if (ammoUI != null)
        {
            ammoUI.weapon = currentWeaponShoot;
        }

        Debug.Log("Weapon ready: " + currentWeaponShoot.name);
    }

    public void EquipWeapon(GameObject weaponPrefab)
    {
        if (weaponSocket == null || weaponPrefab == null)
        {
            Debug.LogWarning("EquipWeapon failed: missing weaponSocket or weaponPrefab.");
            return;
        }

        for (int i = weaponSocket.childCount - 1; i >= 0; i--)
        {
            Destroy(weaponSocket.GetChild(i).gameObject);
        }

        GameObject newWeapon = Instantiate(weaponPrefab, weaponSocket);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;
        newWeapon.transform.localScale = Vector3.one;

        SetupExistingWeapon();
    }
}