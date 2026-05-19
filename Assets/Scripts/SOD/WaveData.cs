using System;
using UnityEngine;

[Serializable]
public class EnemySpawnEntry
{
    public GameObject prefab;
    public int count;
}

[Serializable]
public class EnemyScalingConfig
{
    public GameObject prefab;
    public int startRound;
    public int baseCount;
    public float countPerRound;
}

[Serializable]
public class RoundOverride
{
    public int round;
    public EnemySpawnEntry[] enemies;
}

[CreateAssetMenu(fileName = "WaveData", menuName = "Game/WaveData")]
public class WaveData : ScriptableObject
{
    public EnemyScalingConfig[] enemyTypes;
    public RoundOverride[] overrides;
}
