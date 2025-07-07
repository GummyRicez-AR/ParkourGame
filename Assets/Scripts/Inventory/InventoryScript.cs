using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InventoryScript : MonoBehaviour
{
    [Header("Currencies")]
    public int chips;

    private int previousChips;
    public int ChipsAcquiredDuringRun { get; set; }
    [Space]

    public HotbarSlotsScr hotbarSlotsScr;
    public GameObject currentlyActiveItem;
    public int activeItemIndex;
    public List<ItemSlot> items = new();

    [Header("Ammo Display GUI")]
    public GameObject ammoDisplay;
    public TMP_Text clip;
    public TMP_Text reserve;
    public TMP_Text reloading;
    [Space]
    public GameObject worldAmmoDisplay;
    public TMP_Text worldClip;
    public TMP_Text worldReserve;
    public TMP_Text worldReloading;

    private PlayerController plrController;
    private PlayerSettings settings;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        previousChips = chips;
        plrController = GetComponentInParent<PlayerController>();
        settings = plrController.settings;
        currentlyActiveItem = null;
        activeItemIndex = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (previousChips != chips)
        {
            if (previousChips < chips)
            {
                ChipsAcquiredDuringRun += chips - previousChips;
            }
            previousChips = chips;
        }
        
        for (int i = 0; i < items.Count + 1; i++)
        {
            if (Input.GetKeyDown(i.ToString()) && !plrController.inDialogue)
            {
                if (i <= items.Count)
                {
                    if (activeItemIndex == i) // player is currently holding item that key corresponds to, unequip it
                    {
                        activeItemIndex = -1;
                        UnequipItem(items[i - 1]);
                        currentlyActiveItem = null;
                    } else if (activeItemIndex > 0) // player is currently holding a different item than this one, unequip that and equip the new one
                    {
                        UnequipItem(items[activeItemIndex - 1]);
                        activeItemIndex = i;
                        EquipItem(items[i - 1]);
                    } else // player is currently holding no item, equip this one
                    {
                        activeItemIndex = i;
                        EquipItem(items[i - 1]);
                    }
                }
            }
        }

        if (currentlyActiveItem is not null && currentlyActiveItem.GetComponent<Gun>() is not null)
        {
            if (settings.useWorldCanvasUI)
            {
                worldAmmoDisplay.SetActive(true);
                worldClip.text = currentlyActiveItem.GetComponent<Gun>().GetAmmoInClip().ToString();
                worldReserve.text = currentlyActiveItem.GetComponent<Gun>().GetReserveAmmo().ToString();
            }
            else
            {
                ammoDisplay.SetActive(true);
                clip.text = currentlyActiveItem.GetComponent<Gun>().GetAmmoInClip().ToString();
                reserve.text = currentlyActiveItem.GetComponent<Gun>().GetReserveAmmo().ToString();
            }
            
        } else
        {
            if (settings.useWorldCanvasUI)
                worldAmmoDisplay.SetActive(false);
            else
                ammoDisplay.SetActive(false);
        }
    }

    public void AddItem(ItemData itemData, Vector3 posOffset, GameObject origPickup)
    {
        GameObject newObj = Instantiate(itemData.itemObject, transform);
        newObj.transform.localPosition = posOffset;
        newObj.SetActive(false);

        ItemSlot newSlot = new();
        newSlot.objectInScene = newObj;
        newSlot.itemDataRef = itemData;
        newSlot.inventorySlotID = items.Count + 1;
        items.Add(newSlot);

        if (itemData.itemName == "InfoSheet")
        {
            InfoSheetScr sheetScr = newObj.GetComponent<InfoSheetScr>();
            sheetScr.text.text = origPickup.GetComponentInChildren<TMP_Text>().text;
            sheetScr.image.sprite = origPickup.GetComponentInChildren<UnityEngine.UI.Image>().sprite;

        }

        hotbarSlotsScr.AddSlot(newSlot);
        plrController.upgradeTracker.RefreshEventUpgrades();
    }

    public void RemoveItem(ItemSlot item)
    {
        hotbarSlotsScr.RemoveSlot(item);
        for (int i = item.inventorySlotID; i < items.Count; i++)
        {
            ItemSlot itemInInventory = items[i];
            itemInInventory.inventorySlotID--;
        }

        if (item.objectInScene == currentlyActiveItem)
        {
            currentlyActiveItem = null;
            activeItemIndex = -1;
        }
        
        Destroy(item.objectInScene);
        plrController.upgradeTracker.RefreshEventUpgrades();
        items.Remove(item);
    }

    public void RemoveItem(GameObject objectInScene)
    {
        ItemSlot slotToRemove = null;
        foreach (ItemSlot slot in items)
        {
            if (slot.objectInScene == objectInScene)
            {
                slotToRemove = slot;
                break;
            }
        }

        if (slotToRemove == null)
        {
            return;
            
        }
        
        hotbarSlotsScr.RemoveSlot(slotToRemove);
        for (int i = slotToRemove.inventorySlotID; i < items.Count; i++)
        {
            ItemSlot itemInInventory = items[i];
            itemInInventory.inventorySlotID--;
        }

        if (slotToRemove.objectInScene == currentlyActiveItem)
        {
            currentlyActiveItem = null;
            activeItemIndex = -1;
        }
        
        Destroy(slotToRemove.objectInScene);
        plrController.upgradeTracker.RefreshEventUpgrades();
        items.Remove(slotToRemove);
    }

    private void EquipItem(ItemSlot item)
    {
        item.objectInScene.SetActive(true);
        item.equipped = true;
        currentlyActiveItem = item.objectInScene;
        hotbarSlotsScr.ItemEquipped(item);
    }

    private void UnequipItem(ItemSlot item)
    {
        item.objectInScene.SetActive(false);
        item.equipped = false;
        hotbarSlotsScr.ItemUnequipped(item);
    }
}
