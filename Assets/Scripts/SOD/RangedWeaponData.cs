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

    // 총기 종류별 고유 패시브 식별자. None=미설정(패시브 없음). 에셋마다 Inspector에서 지정.
    public WeaponType weaponType = WeaponType.None;

    [Header("사운드")]
    public AudioClip fireSound;
    public AudioClip reloadSound;
}

// 총기 종류 — 종류별 고유 패시브 식별용. 슬롯 구분(WeaponCategory)과는 별개 층위.
public enum WeaponType
{
    None = 0,
    HG,
    SMG,
    AR,
    SG,
    SR,
}
