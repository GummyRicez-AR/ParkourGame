using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelStatsBehavior : MonoBehaviour
{
    private PlayerController playerController;
    private LevelSpawnScr levelSpawnScr;
    private LevelStatsUI statsUI;

    private List<EnemyCount> enemiesKilledThisRun = new();
    private int targetsHitThisRun = 0;
    private int startingLevels;

    public float timeMultiplier;
    public float timeLeeway = 10;
    public float levelTime;
    public float totalTime;
    public bool countingLevelTime;
    public bool passedTutorialLevels;
    public bool inDanger;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        levelSpawnScr = FindFirstObjectByType<LevelSpawnScr>();
        statsUI = GetComponentInChildren<LevelStatsUI>();
        startingLevels = FindObjectsByType<LevelTemplateScr>(FindObjectsSortMode.None).Length;
        levelTime = totalTime = 0;
        timeMultiplier = 1;
        countingLevelTime = true;
        passedTutorialLevels = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (levelSpawnScr is null)
        {
            return;
        }
        
        totalTime += Time.deltaTime;
        
        if (countingLevelTime && passedTutorialLevels)
        {
            levelTime += Time.deltaTime * timeMultiplier;
            if (levelSpawnScr.runningChipTimeTotal - levelTime >= 0)
            {
                inDanger = false;
            } else
            {
                if (!inDanger)
                {
                    inDanger = true;
                }

                if (levelSpawnScr.runningChipTimeTotal - levelTime + timeLeeway <= 0)
                {
                    EndRun();
                }
            }
        }

        if (!passedTutorialLevels)
        {
            if (levelSpawnScr.currentFarthestLevel.GetComponent<LevelTemplateScr>().roomNumber > startingLevels)
            {
                passedTutorialLevels = true;
            }
        }
    }
    
    public void SafeRoomReached()
    {
        levelTime = 0;
        countingLevelTime = false;
        inDanger = false;
        statsUI.PauseLevelTime();
    }

    public void SafeRoomExited()
    {
        countingLevelTime = true;
    }

    public void EnemyKilled(EnemyInfo info)
    {
        print($"registered: {info.ID}");
        foreach (EnemyCount c in enemiesKilledThisRun)
        {
            if (c.enemyID == info.ID)
            {
                c.count++;
                return;
            }
        }
        enemiesKilledThisRun.Add(new EnemyCount(info.ID));
    }

    public void TargetHit()
    {
        targetsHitThisRun++;
    }
    
    public void ChangeLevelTime(float change) { levelTime += change; }

    public float GetLevelTime() { return levelTime; }

    private void EndRun()
    {
        AddRunData();
        SceneManager.LoadScene("StartScene");
    }

    private void AddRunData()
    {
        RunData newRunData = new();
        newRunData.chipsEarned = playerController.inventory.ChipsAcquiredDuringRun;
        newRunData.totalTime = totalTime;
        newRunData.targetsHit = targetsHitThisRun;
        newRunData.enemiesKilled = enemiesKilledThisRun;
        newRunData.upgradesList = playerController.upgradeTracker.upgradeCount;
        newRunData.roomsCleared = levelSpawnScr.currLevel;
        
        playerController.stats.runDataList.Add(newRunData);
        playerController.stats.chipsEarned += playerController.inventory.ChipsAcquiredDuringRun;
        playerController.stats.targetsHit += targetsHitThisRun;

        foreach (EnemyCount a in enemiesKilledThisRun)
        {
            bool enemyFound = false;
            foreach (EnemyCount b in playerController.stats.enemiesKilled)
            {
                if (a.enemyID == b.enemyID)
                {
                    b.count += a.count;
                    enemyFound = true;
                    break;
                }
            }
            
            if (!enemyFound)
                playerController.stats.enemiesKilled.Add(new EnemyCount(a));
        }
    }
}
