using UnityEngine;
using System.Collections.Generic;

public class SafeRoomScr : MonoBehaviour
{
    private int upgradesBoughtThisSafeRoom;
    private GameObject[] upgradeSpawnPoints;
    private List<GameObject> upgrades = new();
    public GameObject upgradePrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        upgradesBoughtThisSafeRoom = 0;
        upgradeSpawnPoints = GameObject.FindGameObjectsWithTag("UpgradeSpawn");
        foreach (GameObject spawnPoint in upgradeSpawnPoints)
        {
            GameObject newUpgrade = Instantiate(upgradePrefab, spawnPoint.transform.position, spawnPoint.transform.rotation * Quaternion.Euler(0, -90, 0), transform);
            upgrades.Add(newUpgrade);
        }
    }

    public void UpgradeChosen(GameObject upgradeObj)
    {
        upgrades.Remove(upgradeObj);
        upgradesBoughtThisSafeRoom++;
        foreach (GameObject upgrade in upgrades)
        {
            UpgradeScr upgradeScr = upgrade.GetComponent<UpgradeScr>();
            upgradeScr.IncreaseCost();
        }

        LevelTemplateScr levelScr = GetComponent<LevelTemplateScr>();
        levelScr.UnlockSafeDoor();
    }
}
