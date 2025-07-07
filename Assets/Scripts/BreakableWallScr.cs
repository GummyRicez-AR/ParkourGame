using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BreakableWallScr : MonoBehaviour
{
    public AudioClip breakSoundFX;
    private AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public IEnumerator BreakWall()
    {
        gameObject.GetComponent<BoxCollider>().enabled = false;
        List<GameObject> fractures = new();
        GameObject origObj = null;

        foreach (Transform obj in transform)
        {
            if (!obj.gameObject.activeInHierarchy && obj.CompareTag("Fracture"))
                fractures.Add(obj.gameObject);
            else if (obj.gameObject.activeInHierarchy && obj.CompareTag("MainObjOfFracture"))
                origObj = obj.gameObject;
        }

        origObj.SetActive(false);
        foreach (GameObject fracture in fractures)
        {
            fracture.SetActive(true);
            fracture.GetComponent<Rigidbody>().AddExplosionForce(100, transform.position, 10);
        }

        source.PlayOneShot(breakSoundFX);
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }
}
