using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using Random = UnityEngine.Random;

public class LevelSpawnScr : MonoBehaviour
{
    private ApplicationDataManager appDataManager;
    
    public float currDifficulty;
    public EnemyDifficulties enemyList;

    [Header("Level Categories")]
    public List<GameObject> easyLevels;
    public List<GameObject> normalLevels;
    public List<GameObject> hardLevels;

    public List<GameObject> possibleHideLevels;

    [Space]
    public GameObject safeRoom;
    public GameObject currentFarthestLevel;
    public GameObject backWall;

    [Header("Level Spawning/Difficulty Parameters")]
    public int intervalBetweenSafeRooms = 20;
    public float difficultyRateIncreasePerSecond = 0.012f;
    public float difficultyIncreaseOnSafeRoom = 0.25f;
    [SerializeField] private float normalDifficultyThreshold = 3;
    [SerializeField] private float hardDifficultyThreshold = 8;
    [SerializeField] private float insaneDifficultyThreshold = 13;

    public float EasyThreshold { get { return 1; } }
    public float NormalThreshold { get; set; }
    public float HardThreshold { get; set; }
    public float InsaneThreshold { get; set; }

    private ThreatSpawnerScr threatSpawner;
    public int currLevel;
    private int roomsBeforeSafeRoom;
    private bool inSafeRoom;

    public int safeRoomsCleared;
    [Space]
    public float runningChipTimeTotal;

    // hiding spot level variables
    public bool spawningHideRoom;
    public int roomsBeforeHide; 

    private IEnumerator Start()
    {
        runningChipTimeTotal = 0;
        inSafeRoom = true;
        currDifficulty = 1;
        roomsBeforeSafeRoom = intervalBetweenSafeRooms - 1;
        spawningHideRoom = false;
        roomsBeforeHide = 0;
        safeRoomsCleared = 0;
        threatSpawner = FindFirstObjectByType<ThreatSpawnerScr>();
        appDataManager = FindFirstObjectByType<ApplicationDataManager>();
        LevelTemplateScr[] startingLevels = FindObjectsByType<LevelTemplateScr>(FindObjectsSortMode.None);
        LevelTemplateScr lowestLevel = startingLevels[0];

        int lowestNum = startingLevels[0].roomNumber;
        foreach (LevelTemplateScr level in startingLevels)
        {
            if (level.roomNumber < lowestNum)
            {
                lowestLevel = level;
                lowestNum = level.roomNumber;
            }
        }

        currentFarthestLevel = lowestLevel.gameObject;
        currLevel = lowestNum;

        while (!appDataManager.loadedSaveData)
        {
            yield return null;
        }
        appDataManager.GetAllLevels(this);
    }

    private void Update()
    {
        if (!inSafeRoom)
        {
            currDifficulty += difficultyRateIncreasePerSecond * Time.deltaTime;
        }
    }

    public void SpawnNewLevel()
    {
        currLevel++;
        if (currLevel == (intervalBetweenSafeRooms * safeRoomsCleared) + 1)
        {
            PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (PlayerController plr in players)
            {
                plr.levelStats.SafeRoomExited();
            }
        }

        roomsBeforeSafeRoom--;
        GameObject spawnPoint = currentFarthestLevel.GetComponent<LevelTemplateScr>().nextLevelSpawnPoint;
        GameObject levelToSpawn;
        if (roomsBeforeSafeRoom < 1)
        {
            roomsBeforeSafeRoom = intervalBetweenSafeRooms;
            levelToSpawn = safeRoom;
            safeRoomsCleared++;
            inSafeRoom = true;
            currDifficulty += difficultyIncreaseOnSafeRoom;

            // get all the players and reset and stop their level timers
            PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (PlayerController plr in players)
            {
                plr.inventory.chips += Mathf.Max((int)(runningChipTimeTotal - plr.levelStats.GetLevelTime()) * 2, 0) + 35;
                runningChipTimeTotal = 0;
                plr.levelStats.SafeRoomReached();
            }
        } else if (spawningHideRoom)
        {
            if (roomsBeforeHide >= 1)
            {
                levelToSpawn = PickLevelBasedOnDifficulty();
                inSafeRoom = false;
                roomsBeforeHide--;
            } else
            {
                spawningHideRoom = false;
                roomsBeforeHide = 0;
                levelToSpawn = possibleHideLevels[Random.Range(0, possibleHideLevels.Count)];
            }
        } else
        {
            levelToSpawn = PickLevelBasedOnDifficulty();
            inSafeRoom = false;
        }
        
        // before spawning in the new level, add the chip time to the running total
        if (levelToSpawn != safeRoom)
        {
            float mult = Mathf.Max((float)(-Math.Sqrt(currDifficulty)/4) + 1.45f, 0.1f);
            
            runningChipTimeTotal += currentFarthestLevel.GetComponent<LevelTemplateScr>().chipTimeThreshold * mult;
        }

        GameObject newLevel = Instantiate(levelToSpawn, spawnPoint.transform.position, spawnPoint.transform.rotation);
        currentFarthestLevel = newLevel;
        currentFarthestLevel.GetComponent<LevelTemplateScr>().roomNumber = currLevel;

        threatSpawner.TickDownThreats();

        DespawnPreviousLevels(3);
    }

    private GameObject PickLevelBasedOnDifficulty()
    {
        if (currDifficulty < normalDifficultyThreshold) // easy - normal
        {
            if (Random.Range(1, normalDifficultyThreshold) < currDifficulty && safeRoomsCleared >= 2)
            {
                return normalLevels[Random.Range(0, normalLevels.Count)];
            } else
            {
                return easyLevels[Random.Range(0, easyLevels.Count)];
            }
        } else
        {
            return normalLevels[Random.Range(0, normalLevels.Count)];
        }
    }

    public void IncrementLevel(GameObject levelPassed)
    {
        currLevel++;
        roomsBeforeSafeRoom--;
        foreach (LevelTemplateScr lvl in FindObjectsByType<LevelTemplateScr>(FindObjectsSortMode.None))
        {
            if (currLevel == lvl.roomNumber)
                currentFarthestLevel = lvl.gameObject;
        }
    }

    public void SpawnInfoSheetForThreat(GameObject infoSheet, Threat threatInfo)
    {
        GameObject sheetInScene = Instantiate(infoSheet, currentFarthestLevel.transform);
        sheetInScene.transform.localPosition = new Vector3(0, 5, 10);
        sheetInScene.transform.localRotation = Quaternion.Euler(Random.Range(-20f, 20f), 180, 0);

        sheetInScene.GetComponentInChildren<TMP_Text>().text = threatInfo.infoSheetWarning;
        sheetInScene.GetComponentInChildren<UnityEngine.UI.Image>().sprite = threatInfo.infoSheetImage;
    }

    private void DespawnPreviousLevels(int range) // despawns every level before the range-th last room accessed
    {
        if (currLevel <= range) { return; }

        int levelRangeToDespawn = currLevel - range;
        LevelTemplateScr[] levels = FindObjectsByType<LevelTemplateScr>(FindObjectsSortMode.None);
        List<GameObject> levelsToDespawn = new();

        foreach (LevelTemplateScr level in levels)
        {
            if (level.roomNumber <= levelRangeToDespawn)
                levelsToDespawn.Add(level.gameObject);
        }

        foreach (GameObject levelObj in levelsToDespawn)
        {
            Destroy(levelObj);
        }

        //after deleting previous levels, add a back wall to the last level
        foreach (LevelTemplateScr level in levels)
        {
            if (level.roomNumber == currLevel - range + 1)
            {
                List<Transform> doorWalls = new();
                foreach (Transform t in level.door.transform.parent)
                {
                    if (t.CompareTag("Wall"))
                    {
                        doorWalls.Add(t);
                    }
                }

                float totalWidth = 0;
                foreach (Transform t in doorWalls)
                {
                    totalWidth += t.localScale.x;
                }

                print(totalWidth);
                
                Instantiate(backWall, level.transform.position + new Vector3(-1, 8, 0), level.transform.rotation * Quaternion.Euler(0, -90, 0), level.transform);
                backWall.transform.localScale = new Vector3(backWall.transform.localScale.x, backWall.transform.localScale.y, totalWidth);
            }
        }
    }
}
