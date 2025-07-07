using UnityEngine;

[CreateAssetMenu(fileName = "NPCInfo", menuName = "Scriptable Objects/NPC/NPCInfo")]
public class NPCInfo : ScriptableObject
{
    public new string name;
    public Sprite icon;
    public Color boxFill;
    public Color boxBorder;
    public Color defaultTextColor;
    public AudioClip talkingSFX;
}
