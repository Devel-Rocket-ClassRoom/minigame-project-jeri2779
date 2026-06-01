using UnityEngine;

public abstract class UpgradeData : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    public string description;
    public int price;
    public int maxLevel;
}
