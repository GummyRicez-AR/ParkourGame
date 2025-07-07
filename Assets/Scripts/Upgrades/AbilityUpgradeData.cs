using UnityEngine;

public enum Ability
{
    None,
    DoubleJump,
    Dropkick
}

[CreateAssetMenu(fileName = "AbilityUpgradeData", menuName = "Scriptable Objects/AbilityUpgradeData")]
public class AbilityUpgradeData : UpgradeData
{
    public Ability ability;
    public float abilityBoost;
}
