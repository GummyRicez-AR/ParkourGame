using UnityEngine;

public class ButtonInt : Interactable
{
    public override void Interact(PlayerController player)
    {
        Debug.Log("Button interacted with: " + gameObject.name);
    }
}
