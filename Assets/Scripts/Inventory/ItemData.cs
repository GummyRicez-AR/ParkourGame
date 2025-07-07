using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class ItemData: ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject itemObject;
    public int itemID;
}
