using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WallScr : MonoBehaviour
{
    public DecalProjector decalProjector;

    public WallDecals decalList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        decalProjector = GetComponentInChildren<DecalProjector>();
        
        if (decalProjector == null)
        {
            return;
        }
        
        if (Random.Range(0f, 1f) > 0.7f)
        {
            decalProjector.enabled = false;
            return;
        }

        decalProjector.enabled = true;
        decalProjector.transform.localPosition = new Vector3(-0.85f, Random.Range(-0.05f, -0.3f), Random.Range(-0.2f, 0.2f));
        decalProjector.material = decalList.decals[Random.Range(0, decalList.decals.Count)];
    }
}
