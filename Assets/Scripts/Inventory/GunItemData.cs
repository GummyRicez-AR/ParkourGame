using UnityEngine;

public enum GunAbility
{
    None,
    ChipToss
}

[CreateAssetMenu(fileName = "GunItem", menuName = "Scriptable Objects/GunItem")]
public class GunItemData : ItemData
{
    [Header("Gun Stats")]
    public float fireRate = 0.25f;
    public int maxAmmo = 200;
    public int clipSize = 8;
    public float reloadTime = 1;
    public float recoilStrength = 10;
    public float damage = 20;
    public GameObject lineObject;
    public AudioClip fireSFX;
    public float headshotMult = 1.5f;
    public bool autoFire = false;
    public GunAbility gunAbility;

    [Header("Gun Abilities (these don't have to be filled if the gun has no ability")]
    public GameObject chipTossObj;
}
