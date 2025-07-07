using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "EnemyInfo", menuName = "Scriptable Objects/EnemyInfo")]
public class EnemyInfo : ScriptableObject
{
    public string ID;
    
    [Space]
    public string enemyName;
    public GameObject enemyObject;
    public float partDifficulty;
    public Sprite icon;
    public bool flying;
}
