using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Scriptable Objects/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    private void OnEnable() => hideFlags = HideFlags.DontUnloadUnusedAsset;
    
    [Header("Volume Settings")]
    public float masterVolume;
    public float soundFXVolume;
    public float bgMusicVolume;

    [Header("Music Settings")]
    public string playerBGMusic;
    [Tooltip("Sets music choice by default in case there is no save data to read from.")]
    public AudioClip defaultBGMusic;
    
    [Header("Keybind Settings")]
    public KeyCode jump;
    public KeyCode sprint;
    public KeyCode crouch;
    public KeyCode interact;
    public KeyCode useItem;
    public KeyCode itemSpecial;

    [Space]
    public bool useWorldCanvasUI;
}
