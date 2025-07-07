using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class UpgradeScr : MonoBehaviour
{
    private bool failedPurchaseFlashing;
    public PossibleUpgrades possibleUpgrades;
    private UpgradeData chosenUpgrade;
    public float currentChipCost;
    public AudioClip failedPurchaseSoundEffect;
    private AudioSource audioSource;

    [Header("Label References")]
    public TMP_Text title;
    public DecalProjector decal;
    public TMP_Text description;
    public TMP_Text costUI;
    public GameObject costCube;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        failedPurchaseFlashing = false;

        if (Random.Range(0f, 1f) >= 0.2f)
        {
            chosenUpgrade = possibleUpgrades.upgradeList[Random.Range(0, possibleUpgrades.upgradeList.Count)];
        }
        else
        {
            chosenUpgrade = possibleUpgrades.abilityUpgradeList[Random.Range(0, possibleUpgrades.abilityUpgradeList.Count)];
        }
        
        currentChipCost = chosenUpgrade.baseCost;
        costUI.text = currentChipCost.ToString();
        title.text = chosenUpgrade.title;
        decal.material = chosenUpgrade.iconDecal;
        description.text = chosenUpgrade.description;
    }

    public void IncreaseCost()
    {
        currentChipCost += chosenUpgrade.baseCost;
        costUI.text = currentChipCost.ToString();
    }

    public IEnumerator CostFlashRed(float decayTime)
    {
        if (failedPurchaseFlashing)
            yield break;

        failedPurchaseFlashing = true;
        costUI.color = Color.red;
        Vector3 origCubeScale = costCube.transform.localScale;
        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime / decayTime;
            costUI.color = new Color(1, progress, progress);
            costCube.transform.localScale = origCubeScale + ((1 - progress) * 0.25f * origCubeScale);
            yield return null;
        }

        costUI.color = Color.white;
        costCube.transform.localScale = origCubeScale;
        failedPurchaseFlashing = false;
    }

    public UpgradeData GetUpgrade() { return chosenUpgrade; }
}
