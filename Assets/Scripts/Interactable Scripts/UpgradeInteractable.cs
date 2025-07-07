using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Compatibility;


public class UpgradeInteractable : Interactable
{
    private SafeRoomScr safeRoomScr;
    private UpgradeScr upgradeScr;
    private UpgradeData data;

    public override void Start()
    {
        base.Start();
        safeRoomScr = FindFirstObjectByType<SafeRoomScr>();
        upgradeScr = GetComponent<UpgradeScr>();
        data = upgradeScr.GetUpgrade();
        StartCoroutine(MoveCycle());
    }

    public override void Update()
    {
        base.Update();
        data ??= upgradeScr.GetUpgrade();
    }

    public override void Interact(PlayerController player)
    {
        base.Interact(player);
        if (player.inventory.chips >= upgradeScr.currentChipCost)
        {
            player.inventory.chips -= (int)upgradeScr.currentChipCost;

            canBeInteractedWith = false;
            interactableUIScript.InteractableOutOfRange(gameObject);
            if (safeRoomScr != null)
            {
                safeRoomScr.UpgradeChosen(gameObject);
            }
            
            AddStats(player);
            
            Destroy(gameObject);
            
        } else
        {
            print("failed to purchase upgrade");
            StartCoroutine(upgradeScr.CostFlashRed(1));
        }
    }

    private void AddStats(PlayerController plr)
    {
        print($"registering: {data.ID}");
        PlayerUpgrades plrUpgrades = plr.upgradeTracker;

        plrUpgrades.AddUpgrade(data);

        AbilityUpgradeData abilityUpgrade;
        try
        {
            abilityUpgrade = (AbilityUpgradeData)data;
        }
        catch (InvalidCastException e)
        {
            abilityUpgrade = null;
        }
        
        if (abilityUpgrade is not null) 
        {
            switch (abilityUpgrade.ability)
            {
                case Ability.Dropkick:
                    if (plr.abilitiesObject.GetComponent<Dropkick>() is null)
                        plr.abilitiesObject.AddComponent<Dropkick>();
                    else
                    {
                        plr.abilitiesObject.GetComponent<Dropkick>().dropkickBoost += abilityUpgrade.abilityBoost;
                    }
                    
                    break;
            }   
        } else if (data.GetType() == typeof(UpgradeData))
        {
            foreach (StatChange change in data.GetStatChanges())
            {
                plrUpgrades.statBoosts.AddBoost(change);
            } 
        }
        
        plr.RefreshActualMovementValues();
        plr.upgradeTracker.RefreshEventUpgrades();
    }

    private IEnumerator MoveCycle()
    {
        float cycleTime = 0;
        Vector3 initPos = transform.localPosition;
        while (gameObject != null)
        {
            cycleTime += Time.deltaTime * 2.5f;
            transform.localPosition = initPos + new Vector3(0, Mathf.Sin(cycleTime) * 0.35f, 0);
            if (cycleTime >= (2 * Mathf.PI))
            {
                cycleTime = 0;
            }
            yield return null;
        }
    }
}
