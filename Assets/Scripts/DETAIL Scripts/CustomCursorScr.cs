using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CustomCursorScr : MonoBehaviour
{
    public List<Sprite> gunCursor;
    public Sprite defaultCursor;
    public float timeBetweenSpriteSwitch;

    private PlayerController plrController;
    private InventoryScript inventoryScr;
    private bool cycleRebound;
    public Image imgComponent;
    private int gunCursorCycle;
    private float spriteTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        plrController = FindFirstObjectByType<PlayerController>();
        inventoryScr = plrController.inventory;
        spriteTime = 0;
        gunCursorCycle = 0;
        cycleRebound = false;
    }

    // Update is called once per frame
    void Update()
    {
        imgComponent.SetNativeSize();
        if (inventoryScr.currentlyActiveItem != null && inventoryScr.currentlyActiveItem.GetComponent<Gun>() != null)
        {
            if (imgComponent.sprite == defaultCursor) //switch cursor immediately upon equipping gun
                imgComponent.sprite = gunCursor[gunCursorCycle];

            spriteTime += Time.deltaTime;
            if (spriteTime > timeBetweenSpriteSwitch)
            {
                spriteTime = 0;
                if (cycleRebound)
                {
                    gunCursorCycle--;
                    if (gunCursorCycle <= 0)
                    {
                        gunCursorCycle = 0;
                        cycleRebound = false;
                    }
                }
                else
                {
                    gunCursorCycle++;
                    if (gunCursorCycle >= gunCursor.Count)
                    {
                        gunCursorCycle = gunCursor.Count - 1;
                        cycleRebound = true;
                    }
                }

                imgComponent.sprite = gunCursor[gunCursorCycle];
            }
        } else
        {
            imgComponent.sprite = defaultCursor;
        }
    }
}
