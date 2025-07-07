using UnityEngine;
using System.Collections.Generic;

public class ChipScr : MonoBehaviour
{
    public float chipGravityScaleAddition;
    public PlayerController playerThatThrewChip;
    public AudioClip coinFlipSfx;
    public AudioClip dingSfx;
    public bool shot;
    public bool landed;
    public GameObject bulletTrail;

    private Rigidbody rb;
    private AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        landed = false;
        shot = false;
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        // apply a large vertical force and a small horizontal force depending on the object's rotation
        rb.AddForce(((Vector3.up * (1150 - (8 * playerThatThrewChip.GetMovementVector().z))) + (transform.rotation * Vector3.forward * (400 + (45 * playerThatThrewChip.GetMovementVector().z)))) * rb.mass);
        rb.AddTorque(new Vector3(Random.Range(-30f, 30f), Random.Range(-30f, 30f), 0));

        audioSource.PlayOneShot(coinFlipSfx);
    }

    // Update is called once per frame
    void Update()
    {
        if (!landed && !shot)
        {
            rb.linearVelocity += Vector3.down * chipGravityScaleAddition;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            landed = true;
            Destroy(gameObject, 1);
        }
    }

    public void ChipShot(Gun gunFired, PlayerController playerFired)
    {
        if (shot) { return; }

        shot = true;
        rb.useGravity = false;
        /*
        foreach (Transform t in transform)
        {
            if (t.GetComponent<Rigidbody>() is not null)
                t.GetComponent<Rigidbody>().AddExplosionForce(500, transform.position, 2);
        }
        */

        TargetScr[] allTargets = FindObjectsByType<TargetScr>(FindObjectsSortMode.None);
        if (allTargets.Length <= 0) { return; }

        if (landed)
        {
            TargetScr target = allTargets[0];
            BulletTrailScr newTrail = Instantiate(bulletTrail, transform.position, transform.rotation).GetComponent<BulletTrailScr>();
            newTrail.target = target.transform.position;
            target.GunShot(gunFired.gunItemSO.damage, playerFired);
            playerFired.inventory.chips++;
        } else
        {
            TargetScr[] newTargetArray = new TargetScr[2];
            for (int i = 0; i < newTargetArray.Length; i++)
            {
                if (i >= allTargets.Length)
                {
                    break;
                }
                newTargetArray[i] = allTargets[i];
            }
            
            foreach (TargetScr target in newTargetArray)
            {
                BulletTrailScr newTrail = Instantiate(bulletTrail, transform.position, transform.rotation).GetComponent<BulletTrailScr>();
                newTrail.target = target.transform.position;
                target.GunShot(gunFired.gunItemSO.damage * 1.5f, playerFired);
                playerFired.inventory.chips++;
            }
        }
        Destroy(gameObject);
    }
}
