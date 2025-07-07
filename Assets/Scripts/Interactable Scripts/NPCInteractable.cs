using UnityEngine;

public class NPCInteractable : Interactable
{
    public DialogueTree dialogueTreeRef;
    public GameObject dialogueCanvasPrefab;

    public override void Interact(PlayerController player)
    {
        base.Interact(player);
        player.inventory.hotbarSlotsScr.enabled = false;
        player.inDialogue = true;
        DialogueScript newDialogue = Instantiate(dialogueCanvasPrefab, player.transform.position, Quaternion.Euler(Vector3.zero)).GetComponent<DialogueScript>();
        newDialogue.dialogueTreeRef = dialogueTreeRef;
        newDialogue.playerInDialogue = player;
        newDialogue.interactableScr = this;
        canBeInteractedWith = false;
    }

    public virtual void DialogueFinished(DialogueScript dialogueScript)
    {
        dialogueScript.playerInDialogue.inventory.hotbarSlotsScr.enabled = true;
        dialogueScript.playerInDialogue.inDialogue = false;
        canBeInteractedWith = true;
    }
}
