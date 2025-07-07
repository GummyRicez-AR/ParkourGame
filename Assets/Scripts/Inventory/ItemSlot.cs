using UnityEngine;

[System.Serializable]
public class ItemSlot
{
    public GameObject objectInScene;
    public ItemData itemDataRef;
    public int inventorySlotID;
    public bool equipped;
}
