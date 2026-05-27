using UnityEngine;

public enum EnemyBehaviorType { Default, Charger, Thrower }

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public float maxHealth = 100f;
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float attackHeightRange = 1.2f;
    public float attackDamage = 10f;
    public float attackInterval = 1f;
    public int moneyReward = 30;
    public int scoreReward = 100;

    public float deathDelay = 3f;

    public EnemyBehaviorType behaviorType = EnemyBehaviorType.Default;
    public float prepareDistance = 8f;
    public float prepareDuration = 1.0f;
    public float recoverDuration = 1.5f;
    public Color prepareColor = Color.red;

    public float chargeSpeed = 8f;
    public float chargeMaxDuration = 1.5f;
    public float chargeContactDamage = 25f;

    public GameObject projectilePrefab;
    public float throwForce = 12f;
    public float projectileDamage = 15f;
    public float fuseTime = 3f;
    public float explosionRadius = 3f;
    public float minThrowAngle = 15f;
    public float maxThrowAngle = 60f;
}
