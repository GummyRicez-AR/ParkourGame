using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyDifficulties", menuName = "Scriptable Objects/EnemyDifficulties")]
public class EnemyDifficulties : ScriptableObject
{
    public List<EnemyInfo> infoList;
}
