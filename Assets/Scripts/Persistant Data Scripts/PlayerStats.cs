using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    private void OnEnable() => hideFlags = HideFlags.DontUnloadUnusedAsset;
    
    public int roomsCleared;
    public int chipsEarned;
    public List<EnemyCount> enemiesKilled = new();
    public int targetsHit;
    
    public List<RunData> runDataList;
}
