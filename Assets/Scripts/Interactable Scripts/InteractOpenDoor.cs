using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SoundFX))]
public class InteractOpenDoor : Interactable
{
    private AudioSource audioSrc;
    public AudioClip openSFX;
    public Material keycardMaterial;
    public ItemData keySORef;

    [Header("Room Parameters")]
    public bool targetsInLevel;
    public bool keycardRequired;
    public bool safeRoomDoor;

    [Space]
    public GameObject doorPart;
    public bool doorOpened = false;

    private bool targetsCleared;

    public override void Start()
    {
        base.Start();
        audioSrc = GetComponent<AudioSource>();
        if (targetsInLevel || safeRoomDoor)
        {
            canBeInteractedWith = false;
        }
        
        if (keycardRequired)
        {
            doorPart.GetComponent<MeshRenderer>().material = keycardMaterial;
        }
    }

    public override void Update()
    {
        base.Update();
        if (targetsInLevel && !targetsCleared)
        {
            int numTargets = GameObject.FindGameObjectsWithTag("Target").Length;
            if (numTargets <= 0)
            {
                targetsCleared = true;
                if (!keycardRequired)
                    Interact(FindFirstObjectByType<PlayerController>());
            }
        }
    }

    public override void Interact(PlayerController player)
    {
        if (keycardRequired)
        {
            foreach (ItemSlot slot in player.inventory.items)
            {
                if (slot.itemDataRef == keySORef)
                {
                    player.inventory.RemoveItem(slot);
                    doorOpened = true;
                    interactableUIScript.InteractableOutOfRange(gameObject);
                    StartCoroutine(DoorOpenTween(0.25f));
                    break;
                }
            }
        } else
        {
            doorOpened = true;
            interactableUIScript.InteractableOutOfRange(gameObject);
            StartCoroutine(DoorOpenTween(0.25f));
        }
    }

    private IEnumerator DoorOpenTween(float time)
    {
        audioSrc.PlayOneShot(openSFX);
        doorPart.GetComponent<Collider>().enabled = false;
        enabled = false;
        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime / time;
            doorPart.transform.Translate(new Vector3(0, ((-2 * progress) + 2) * -0.45f, 0));
            yield return null;
        }
    }

    public void SafeRoomDoorUnlock()
    {
        canBeInteractedWith = true;
    }
}
