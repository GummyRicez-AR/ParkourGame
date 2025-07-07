using UnityEngine;

[CreateAssetMenu(fileName = "Threat", menuName = "Scriptable Objects/Threat")]
public class Threat : ScriptableObject
{
    public string threatName;
    public GameObject threatPrefab;
    public int minRandomRange;
    public int maxRandomRange;

    public Sprite infoSheetImage;
    [TextArea(2, 4)] public string infoSheetWarning;
}
