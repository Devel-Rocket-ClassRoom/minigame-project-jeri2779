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
    // 한 발씩 순차 장전(펌프식). false면 탄창 통째 장전. 펠릿 수와 무관하게 장전 방식만 결정.
    public bool shellReload = false;
}
