using UnityEngine;

public abstract class WeaponData : ScriptableObject
{
    public string displayName;
    public float damage = 25f;
    public float fireRate = 0.1f;
    public float range = 100f;
    public GameObject weaponModelPrefab;
    public int price;
    public float verticalRecoil;
    public WeaponCategory category = WeaponCategory.Primary;
    public bool isAutomatic = true;
    public Vector3 viewModelPosition;
    public Vector3 viewModelRotation;
    public float drawDuration = 0.5f;
    public Sprite weaponIcon;
}

public enum WeaponCategory
{
    Primary = 0,
    Secondary = 1,
    Melee = 2,
    Throwable = 3,
}
