using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    public TextMeshProUGUI ammoText;

    private WeaponShoot currentWeapon;

    public void SetWeapon(WeaponShoot newWeapon)
    {
        currentWeapon = newWeapon;
        UpdateAmmo();
    }

    void Update()
    {
        UpdateAmmo();
    }

    void UpdateAmmo()
    {
        if (ammoText == null) return;

        if (currentWeapon == null)
        {
            ammoText.text = "";
            return;
        }

        ammoText.text = currentWeapon.ammoInMag + " / " + currentWeapon.reserveAmmo;
    }
}