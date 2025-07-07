using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HotbarSlotsScr : MonoBehaviour
{
    public InventoryScript inventory;
    public GameObject slotImg;

    // GameObject is the slot image, while ItemSlot is the class that contains the actual item object in scene
    private Dictionary<GameObject, ItemSlot> slots = new();

    private void OnEnable()
    {
        StartCoroutine(MoveCycle());
    }

    public void AddSlot(ItemSlot item)
    {
        GameObject newSlot = Instantiate(slotImg, transform);
        InventorySlotImgScr imgScr = newSlot.GetComponent<InventorySlotImgScr>();
        imgScr.image.sprite = item.itemDataRef.icon;
        imgScr.keyToPress.text = item.inventorySlotID.ToString();
        imgScr.background.color = new Color(0.5f, 0.5f, 0.5f);
        imgScr.image.color = new Color(0.5f, 0.5f, 0.5f);
        slots.Add(newSlot, item);
        RepositionSlots();
    }

    public void RemoveSlot(ItemSlot item)
    {
        List<GameObject> keys = new List<GameObject>(slots.Keys);
        List<ItemSlot> values = new List<ItemSlot>(slots.Values);

        for (int i = 0; i < keys.Count; i++)
        {
            if (values[i] == item)
            {
                slots.Remove(keys[i]);
                Destroy(keys[i]); 
            }
        }

        RepositionSlots();
    }

    private void RepositionSlots()
    {
        float currXPos = -85 * (slots.Count - 1);
        foreach (GameObject slot in slots.Keys)
        {
            slot.transform.localPosition = new Vector3(currXPos, 0, 0);
            currXPos += 175;
        }
    }

    public void ItemEquipped(ItemSlot item)
    {
        foreach (KeyValuePair<GameObject, ItemSlot> pair in slots)
        {
            if (pair.Value == item)
            {
                pair.Key.GetComponent<InventorySlotImgScr>().background.color = new Color(1, 1, 1);
                pair.Key.GetComponent<InventorySlotImgScr>().image.color = new Color(1, 1, 1);
                break;
            }
        }
    }

    public void ItemUnequipped(ItemSlot item)
    {
        foreach (KeyValuePair<GameObject, ItemSlot> pair in slots)
        {
            if (pair.Value == item)
            {
                pair.Key.GetComponent<InventorySlotImgScr>().background.color = new Color(0.5f, 0.5f, 0.5f);
                pair.Key.GetComponent<InventorySlotImgScr>().image.color = new Color(0.5f, 0.5f, 0.5f);
                break;
            }
        }
    }

    private IEnumerator MoveCycle()
    {
        float cycleTime = 0;
        Vector3 initPos = transform.localPosition;
        while (true)
        {
            cycleTime += Time.deltaTime * 2f;
            transform.localPosition = initPos + new Vector3(0, Mathf.Sin(cycleTime) * 10, 0);
            yield return null;
        }
    }
}
