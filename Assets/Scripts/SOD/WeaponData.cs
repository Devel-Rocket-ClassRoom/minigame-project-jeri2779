using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    // Add weapon properties here
    public float damage = 25f;
    public float fireRate = 0.1f;
    public float range = 100f;
    public int magazineSize = 30;

    public float reloadTime = 1.5f;

    public GameObject weaponModelPrefab;
    public int price;
    public float verticalRecoil;
    public int maxReserveAmmo;

    public WeaponCategory category = WeaponCategory.Primary;
    public bool isAutomatic = true;
    public int pelletCount = 1;
    public float spreadAngle = 0f;

    public Vector3 viewModelPosition;
    public Vector3 viewModelRotation;
    public float drawDuration = 0.5f;
}

public enum WeaponCategory
{
    Primary = 0,
    Secondary = 1,
    Melee = 2,
    Throwable = 3,
}
