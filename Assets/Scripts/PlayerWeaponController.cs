using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Refs")]
    public Transform weaponSocket;
    public Camera playerCam;
    public WeaponADS weaponADS;
    public AmmoUI ammoUI;
    public HitMarkerUI hitMarkerUI; // optional: assign in inspector

    private WeaponShoot currentWeaponShoot;
    private GameObject currentWeaponObject;

    void Awake()
    {
        if (weaponADS == null) weaponADS = GetComponentInChildren<WeaponADS>();
        if (playerCam == null) playerCam = GetComponentInChildren<Camera>();
    }

    void Start()
    {
        DisableAllWeaponShoots();
        SetupExistingWeapon();
    }

    void DisableAllWeaponShoots()
    {
        foreach (var w in FindObjectsByType<WeaponShoot>(FindObjectsSortMode.None))
            w.enabled = false;
    }

    public void SetupExistingWeapon()
    {
        if (weaponSocket == null)
        {
            Debug.LogError("PlayerWeaponController: weaponSocket not assigned!");
            return;
        }

        currentWeaponShoot = weaponSocket.GetComponentInChildren<WeaponShoot>();
        currentWeaponObject = currentWeaponShoot != null ? currentWeaponShoot.gameObject : null;

        if (currentWeaponShoot == null)
        {
            Debug.LogWarning("No WeaponShoot found in weaponSocket.");
            return;
        }

        ActivateWeapon(currentWeaponShoot);
    }

    public void EquipWeapon(GameObject newWeaponPrefab)
    {
        if (newWeaponPrefab == null || weaponSocket == null)
        {
            Debug.LogWarning("EquipWeapon failed: missing prefab or weaponSocket.");
            return;
        }

        if (currentWeaponShoot != null) currentWeaponShoot.enabled = false;
        if (currentWeaponObject != null) Destroy(currentWeaponObject);

        GameObject instance = Instantiate(newWeaponPrefab, weaponSocket);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        currentWeaponObject = instance;
        currentWeaponShoot = instance.GetComponentInChildren<WeaponShoot>();

        if (currentWeaponShoot == null)
        {
            Debug.LogError("New weapon prefab has no WeaponShoot script!");
            return;
        }

        ActivateWeapon(currentWeaponShoot);
        Debug.Log($"Equipped: {instance.name}");
    }

    void ActivateWeapon(WeaponShoot ws)
    {
        ws.enabled = true;
        ws.playerCam = playerCam;

        // FIX: tell WeaponADS about the new weapon's hip/ADS poses so ADS works after swapping
        if (weaponADS != null)
            weaponADS.SetPoses(ws.hipPos, ws.adsPos);

        // event-driven ammo UI
        if (ammoUI != null)
            ammoUI.SetWeapon(ws);

        // hook up hit markers
        if (hitMarkerUI != null)
            hitMarkerUI.SetWeapon(ws);
    }
}