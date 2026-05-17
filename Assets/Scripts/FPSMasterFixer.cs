using UnityEngine;

[ExecuteInEditMode]
public class FPSMasterFixer : MonoBehaviour
{
    [Header("1P Setup")]
    public Transform viewmodelArms;
    public Transform viewmodelGun;
    public Vector3 armsOffset = new Vector3(0.12f, -0.35f, 0.3f);
    public Vector3 gunRotation = new Vector3(90, 0, 0);

    [Header("3P Setup")]
    public Transform worldGun;
    public Transform handR;
    public Vector3 worldGunRotation = new Vector3(90, 0, 0);

    void LateUpdate()
    {
        // Force 1P Arms to Camera
        if (viewmodelArms != null)
        {
            viewmodelArms.localPosition = armsOffset;
            viewmodelArms.localRotation = Quaternion.identity;
        }

        // Force 1P Gun rotation
        if (viewmodelGun != null)
        {
            viewmodelGun.localRotation = Quaternion.Euler(gunRotation);
            viewmodelGun.localPosition = Vector3.zero;
        }

        // Force 3P Gun to Hand
        if (worldGun != null && handR != null)
        {
            worldGun.SetParent(handR);
            worldGun.localPosition = Vector3.zero;
            worldGun.localRotation = Quaternion.Euler(worldGunRotation);
        }
    }
}
