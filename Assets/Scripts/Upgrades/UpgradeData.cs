using UnityEngine;
using System.Collections.Generic;

public enum Stat
{
    None,
    WalkSpeed,
    SprintSpeed,
    JumpPower,
    WallBoost,
    ChipOnHit,
    FireRate,
    ReloadSpeed,
}

[System.Serializable]
[CreateAssetMenu(fileName = "Upgrade", menuName = "Scriptable Objects/Upgrade")]
public class UpgradeData : ScriptableObject
{
    public string ID;
    
    [Space]
    public string title;
    public Sprite icon;
    public Material iconDecal;
    [SerializeField] private List<StatChange> statChanges;
    public float baseCost = 50;

    [TextArea(5, 10)]
    public string description;

    public List<StatChange> GetStatChanges()
    {
        return statChanges;
    }
}
