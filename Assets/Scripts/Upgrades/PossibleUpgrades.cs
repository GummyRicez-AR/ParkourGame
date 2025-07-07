using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PossibleUpgrades", menuName = "Scriptable Objects/PossibleUpgrades")]
public class PossibleUpgrades : ScriptableObject
{
    public List<UpgradeData> upgradeList;
    public List<AbilityUpgradeData> abilityUpgradeList;
}
