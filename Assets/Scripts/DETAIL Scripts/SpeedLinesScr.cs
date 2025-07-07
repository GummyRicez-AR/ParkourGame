using UnityEngine;

public class SpeedLinesScr : MonoBehaviour
{
    private PlayerController plrController;
    public ParticleSystem circleEmitter;
    
    private new ParticleSystem particleSystem;
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.VelocityOverLifetimeModule velocityModule;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        plrController = GetComponentInParent<PlayerController>();
        circleEmitter = GetComponentInChildren<ParticleSystem>();
        particleSystem = GetComponent<ParticleSystem>();
        velocityModule = particleSystem.velocityOverLifetime;
        emissionModule = particleSystem.emission;
    }

    // Update is called once per frame
    private void Update()
    {
        float trueZ = plrController.GetTrueMovementVector().z;
        
        if (trueZ > plrController.ActualSprintSpeed)
        {
            if (particleSystem.isStopped)
                particleSystem.Play();
            
            emissionModule.rateOverTime = 12 + (trueZ / 4);
            velocityModule.z = trueZ;

            transform.localPosition = plrController.GetTrueMovementVector().normalized * (10 + (trueZ / 6));
            transform.rotation = transform.parent.rotation * Quaternion.LookRotation(-plrController.GetTrueMovementVector().normalized);
        }
        else
        {
            if (particleSystem.isPlaying)
                particleSystem.Stop(true);
        }
    }
}
