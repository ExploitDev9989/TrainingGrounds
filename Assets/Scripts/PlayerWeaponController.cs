using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Refs")]
    public Transform weaponSocket;      // point on the player where the weapon is attached
    public Camera playerCam;            // player camera used for aiming
    public WeaponADS weaponADS;         // reference to ADS script that moves the weapon when aiming

    private WeaponShoot currentWeaponShoot; // stores the weapon script found on the equipped weapon

    void Awake()
    {
        // if ADS script wasn't manually assigned, try to find it on children
        if (weaponADS == null)
            weaponADS = GetComponentInChildren<WeaponADS>();

        // same idea here — automatically find the camera if it wasn't assigned
        if (playerCam == null)
            playerCam = GetComponentInChildren<Camera>();
    }

    void Start()
    {
        // when the game starts, set up the weapon that is already attached to the player
        SetupExistingWeapon();
    }

    void SetupExistingWeapon()
    {
        // safety
        if (weaponSocket == null)
        {
            Debug.LogError("PlayerWeaponController: weaponSocket not assigned!");
            return; 
        }

        // look for a WeaponShoot script on the weapon that is inside the socket
        currentWeaponShoot = weaponSocket.GetComponentInChildren<WeaponShoot>();

        // if no weapon script is found we warn in the console
        if (currentWeaponShoot == null)
        {
            Debug.LogWarning("No WeaponShoot found under weaponSocket.");
            return;
        }

        // give the weapon the player camera reference so it knows where to aim
        currentWeaponShoot.playerCam = playerCam;

        // make sure the weapon script is enabled so it can shoot
        currentWeaponShoot.enabled = true;

        // connect the ADS system to the weapon poses if they exist
        if (weaponADS != null &&
            currentWeaponShoot.hipPos != null &&
            currentWeaponShoot.adsPos != null)
        {
            // pass the hip and ADS transforms to the WeaponADS script
            weaponADS.SetPoses(currentWeaponShoot.hipPos, currentWeaponShoot.adsPos, snapToHip: true);
        }

        // print message in console so we know the weapon initialized correctly
        Debug.Log("Weapon ready: " + currentWeaponShoot.name);
    }
}