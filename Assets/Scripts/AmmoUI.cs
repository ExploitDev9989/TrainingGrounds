using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    public WeaponShoot weapon;     // drag your weapon (the one with WeaponShoot) here
    public TMP_Text ammoText;      // drag AmmoText (TMP) here

    void Update()
    {
        if (weapon == null || ammoText == null) return;

        ammoText.text = $"{weapon.ammoInMag} / {weapon.magSize}   |   {weapon.reserveAmmo}";
        // Example: 7 / 12 | 48
    }
}