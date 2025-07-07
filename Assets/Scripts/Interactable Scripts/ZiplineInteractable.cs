using UnityEngine;

public class ZiplineInteractable : Interactable
{
    public Zipline ziplineScr;
    public override void Interact(PlayerController player)
    {
        base.Interact(player);
        StartCoroutine(ziplineScr.StartRidingZipline(player));
    }
}
