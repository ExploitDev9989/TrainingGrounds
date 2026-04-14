using UnityEngine;

public class WeaponADS : MonoBehaviour
{
    public Transform weaponSocket;     // the weapon object we move (usually WeaponSocket under the viewmodel)
    public Transform hipPos;           // pose for hip-fire (not aiming)
    public Transform adsPos;           // pose for ADS (aiming down sights)
    public float adsSpeed = 12f;       // how fast we transition between hip and ADS
    //public Transform target;

    void Start()
    {
        // start the weapon at hip position so it doesn't spawn in ADS by accident
        SnapTo(hipPos);
    }

    void Update()
    {
        // safety check (prevents null reference errors)
        if (weaponSocket == null || hipPos == null || adsPos == null) return;

        // right mouse button = aim (ADS), otherwise stay hip
        Transform target = Input.GetMouseButton(1) ? adsPos : hipPos;

        // smoothly move weaponSocket toward the target pose
        MoveTowards(target);
    }

    void MoveTowards(Transform target)
    {
        // we move the weaponSocket in LOCAL space,
        // but our target poses are in WORLD space (because they are placed in the scene/viewmodel)
        Transform parent = weaponSocket.parent;
        if (parent == null) return; // safety check

        // convert the target WORLD position into the parent's LOCAL position
        Vector3 targetLocalPos = parent.InverseTransformPoint(target.position);

        // convert the target WORLD rotation into the parent's LOCAL rotation
        Quaternion targetLocalRot = Quaternion.Inverse(parent.rotation) * target.rotation;

        // smoothly move position toward the target (Lerp = smooth blend)
        weaponSocket.localPosition = Vector3.Lerp(
            weaponSocket.localPosition,
            targetLocalPos,
            Time.deltaTime * adsSpeed
        );

        // smoothly rotate toward the target (Slerp = smooth rotation blend)
        weaponSocket.localRotation = Quaternion.Slerp(
            weaponSocket.localRotation,
            targetLocalRot,
            Time.deltaTime * adsSpeed
        );
    }

    void SnapTo(Transform target)
    {
        // safety checks
        if (weaponSocket == null || target == null) return;

        Transform parent = weaponSocket.parent;
        if (parent == null) return;

        // instantly set weapon socket to the target pose (no smoothing)
        weaponSocket.localPosition = parent.InverseTransformPoint(target.position);
        weaponSocket.localRotation = Quaternion.Inverse(parent.rotation) * target.rotation;
    }

    public void SetPoses(Transform newHip, Transform newAds, bool snapToHip = true)
    {
        // lets another script assign the hip + ads poses for the current weapon
        hipPos = newHip;
        adsPos = newAds;

        // optional: snap right away so it doesn't slowly drift from a wrong start position
        if (snapToHip) SnapTo(hipPos);
    }
    void OnDrawGizmos()
    {
        if (hipPos != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hipPos.position, 0.02f);
            Gizmos.DrawLine(hipPos.position, hipPos.position + hipPos.forward * 0.2f);
        }

        if (adsPos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(adsPos.position, 0.02f);
            Gizmos.DrawLine(adsPos.position, adsPos.position + adsPos.forward * 0.2f);
        }

        if (weaponSocket != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(weaponSocket.position, 0.02f);
            Gizmos.DrawLine(weaponSocket.position, weaponSocket.position + weaponSocket.forward * 0.2f);
        }
    }
}