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

    public Vector3 swingEulerAxis = Vector3.up;
    public float swingAngle = 60f;
    public float thrustDistance = 0.15f;
    public Vector3 thrustPoseRotation = new Vector3(75f, 10f, 15f);
    [Range(0.1f, 0.9f)] public float thrustRotationRatio = 0.35f;
}
