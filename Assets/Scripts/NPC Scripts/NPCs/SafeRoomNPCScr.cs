using UnityEngine;
using System.Collections.Generic;

public class SafeRoomNPCScr : NPCInteractable
{
    public List<DialogueTree> alternateTrees;

    public override void Start()
    {
        base.Start();
        dialogueTreeRef = alternateTrees[Random.Range(0, alternateTrees.Count)];
    }

    public override void DialogueFinished(DialogueScript dialogueScript)
    {
        base.DialogueFinished(dialogueScript);
        dialogueTreeRef = alternateTrees[Random.Range(0, alternateTrees.Count)];
    }
}
