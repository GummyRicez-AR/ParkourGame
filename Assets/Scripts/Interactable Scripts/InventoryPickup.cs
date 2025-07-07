using UnityEngine;

public class InventoryPickup : Interactable
{
    public ItemData item;
    public Vector3 posOffset;

    public override void Interact(PlayerController player)
    {
        interactableUIScript.InteractableOutOfRange(gameObject);
        player.inventory.GetComponent<InventoryScript>().AddItem(item, posOffset, gameObject);
        Destroy(gameObject);
    }
}
