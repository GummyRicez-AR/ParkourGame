using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StatBoostList
{
    public List<StatChange> list = new();

    public StatChange GetBoost(Stat statToLookFor)
    {
        foreach (StatChange boost in list)
        {
            if (boost.stat == statToLookFor)
            {
                return boost;
            }
        }

        return new StatChange();
    }

    public void AddBoost(StatChange statChange)
    {
        foreach (StatChange boostRef in list)
        {
            if (boostRef.stat == statChange.stat)
            {
                boostRef.additiveChangeAmount += statChange.additiveChangeAmount;
                boostRef.multiplicativeChangeAmount += statChange.multiplicativeChangeAmount;
                return;
            }
        }

        list.Add(new StatChange(statChange));
    }
}

public class PlayerUpgrades : MonoBehaviour
{
    private class UpgradeFunctions
    {
        public static void ChipsOnShot(PlayerController player, int chipReward, Gun gun)
        {
            player.inventory.chips += chipReward;
            gun.GetComponent<AudioSource>().PlayOneShot(gun.gunItemSO.chipTossObj.GetComponent<ChipScr>().dingSfx);
        }
    }
    
    private PlayerController player;
    private ApplicationDataManager appDataManager;
    public List<UpgradeCount> upgradeCount = new();
    public StatBoostList statBoosts  = new();

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
        appDataManager = FindFirstObjectByType<ApplicationDataManager>();
    }
    
    public void AddUpgrade(UpgradeData newUpgrade)
    {
        bool itemInList = false;
        foreach (UpgradeCount entry in upgradeCount)
        {
            if (entry.upgradeID == newUpgrade.ID)
            {
                itemInList = true;
                entry.count++;
            }
        }

        if (!itemInList)
        {
            upgradeCount.Add(new UpgradeCount { upgradeID = newUpgrade.ID, count = 1 });
        }
    }

    public void RefreshEventUpgrades()
    {
        List<Gun> gunItems = new();
        foreach (ItemSlot slot in player.inventory.items)
        {
            if (slot.objectInScene.GetComponent<Gun>() is not null)
            {
                gunItems.Add(slot.objectInScene.GetComponent<Gun>());
            }
        }

        foreach (Gun g in gunItems)
        {
            g.OnShot.RemoveAllListeners();
            if (statBoosts.GetBoost(Stat.ChipOnHit).stat == Stat.ChipOnHit)
            {
                g.OnShot.AddListener(() => UpgradeFunctions.ChipsOnShot(player, (int)statBoosts.GetBoost(Stat.ChipOnHit).additiveChangeAmount, g));
            }
        }
    }
}
