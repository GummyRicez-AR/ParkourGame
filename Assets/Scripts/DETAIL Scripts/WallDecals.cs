using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WallDecals", menuName = "Scriptable Objects/WallDecals")]
public class WallDecals : ScriptableObject
{
    public List<Material> decals;
}
