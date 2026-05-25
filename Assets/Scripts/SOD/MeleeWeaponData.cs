using UnityEngine;

[CreateAssetMenu(fileName = "MeleeWeaponData", menuName = "Scriptable Objects/MeleeWeaponData")]
public class MeleeWeaponData : WeaponData
{
    public float swingWindup = 0f;
    public int maxTargets = 1;
    public float halfAngleX = 90f;
    public float halfAngleY = 90f;
    public float sphereRadius = 2f;

    public float altDamageMultiplier = 2f;
    public float altHalfAngleX = 10f;
    public float altHalfAngleY = 10f;
}
