using UnityEngine;

public class BarLightScr : MonoBehaviour
{
    public new Light light;
    public Material darkEmissive;
    public Material lightEmissive;

    private MeshRenderer meshRenderer;
    private float origLightIntensity;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        origLightIntensity = light.intensity;
    }

    public void BlinkOff()
    {
        light.intensity = 0;
        meshRenderer.material = darkEmissive;
    }

    public void BlinkOn()
    {
        light.intensity = origLightIntensity;
        meshRenderer.material = lightEmissive;
    }
}
