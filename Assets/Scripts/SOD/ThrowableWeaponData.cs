using UnityEngine;

[CreateAssetMenu(fileName = "ThrowableWeaponData", menuName = "Scriptable Objects/ThrowableWeaponData")]
public class ThrowableWeaponData : WeaponData
{
    public float fuseTime = 3f;
    public float blastRadius = 5f;
    public float blastDamage = 100f;
    public GameObject grenadePrefab;
    public float throwForce = 10f;
    public int maxCount = 2;
    public GameObject explosionVFX;
}
