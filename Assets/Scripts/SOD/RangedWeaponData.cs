using UnityEngine;

[CreateAssetMenu(fileName = "RangedWeaponData", menuName = "Scriptable Objects/RangedWeaponData")]
public class RangedWeaponData : WeaponData
{
    public int magazineSize = 30;
    public float reloadTime = 1.5f;
    public int maxReserveAmmo = 90;
    public int pelletCount = 1;
    public float spreadAngle = 0f;
    public bool useScope = false;
}
