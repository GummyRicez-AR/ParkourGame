using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct DialogueChoice
{
    public string playerChoice;
    public DialogueTree resultTree;
}

[System.Serializable]
public class Dialogue
{
    public NPCInfo speaker;
    [TextArea(0, 3)]
    public string dialogue;
    public bool hasChoices;
    public List<DialogueChoice> dialogueChoices;
}

[CreateAssetMenu(fileName = "DialogueTree", menuName = "Scriptable Objects/NPC/DialogueTree")]
public class DialogueTree : ScriptableObject
{
    public List<Dialogue> dialogues;
}
