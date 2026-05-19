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
}
