using System;
using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application = UnityEngine.Application;

public class ApplicationDataManager : MonoBehaviour
{
    public bool deleteDataOnStart;
    
    [Space]
    public PlayerSettings playerSettings;
    public PlayerStats playerStats;
    private string playerDataFilePath;

    public readonly Dictionary<string, UpgradeData> AllUpgrades = new();
    public readonly Dictionary<string, EnemyInfo> AllEnemies = new();
    public readonly Dictionary<string, AudioClip> AllMusicTracks = new();

    public readonly List<GameObject> EasyLevels = new();
    public readonly List<GameObject> NormalLevels = new();
    public readonly List<GameObject> StartingLevels = new();
    public readonly List<GameObject> HideLevels = new();
    public GameObject SafeRoom;
    
    public int upgradesLoaded;
    public int enemiesLoaded;
    public int musicLoaded;
    public int levelsLoaded;
    
    public int assetsLoaded;
    public bool loadedSaveData;
    
    public AsyncOperationHandle<IList<UpgradeData>> upgradeOperation;
    public AsyncOperationHandle<IList<EnemyInfo>> enemyOperation;
    public AsyncOperationHandle<IList<AudioClip>> musicOperation;
    public AsyncOperationHandle<IList<GameObject>> levelOperation;

    private void Awake()
    {
        loadedSaveData = false;
        assetsLoaded = upgradesLoaded = enemiesLoaded = musicLoaded = levelsLoaded = 0;
        if (FindObjectsByType<ApplicationDataManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
        playerDataFilePath = Application.persistentDataPath;

        StartCoroutine(WaitABit());
    }

    private IEnumerator WaitABit()
    {
        yield return new WaitForSecondsRealtime(2f);
        BeginLoadData();
    }

    private async void BeginLoadData()
    {
        float startTime = Time.realtimeSinceStartup;
        upgradeOperation = Addressables.LoadAssetsAsync<UpgradeData>(new List<string> { "upgrades" },
            data =>
            {
                if (data is null) return;
                
                AllUpgrades.Add(data.ID, data);
                assetsLoaded++;
                upgradesLoaded++;
                
            }, Addressables.MergeMode.Union, false);
        await upgradeOperation.Task;

        enemyOperation = Addressables.LoadAssetsAsync<EnemyInfo>(new List<string> { "enemies" },
            info =>
            {
                if (info is null) return;
                
                AllEnemies.Add(info.ID, info);
                assetsLoaded++;
                enemiesLoaded++;
            }, Addressables.MergeMode.Union, false);
        await enemyOperation.Task;

        musicOperation = Addressables.LoadAssetsAsync<AudioClip>(new List<string> { "music" },
            track =>
            {
                if (track is null) return;
                
                AllMusicTracks.Add(track.name, track);
                assetsLoaded++;
                musicLoaded++;
                
            }, Addressables.MergeMode.Union, false);
        await musicOperation.Task;

        levelOperation = Addressables.LoadAssetsAsync<GameObject>(new List<string> { "levels" },
            obj =>
            {
                if (obj is null) return;
                
                switch (obj.GetComponent<LevelTemplateScr>().roomType)
                {
                    case RoomType.Easy:
                        EasyLevels.Add(obj);
                        break;
                    case RoomType.Medium:
                        NormalLevels.Add(obj);
                        break;
                    case RoomType.Hide:
                        HideLevels.Add(obj);
                        break;
                    case RoomType.Safe:
                        SafeRoom = obj;
                        break;
                }
                assetsLoaded++;
                levelsLoaded++;
                
            }, Addressables.MergeMode.Union, false);
        await levelOperation.Task;
        
        print($"operations complete, took {Time.realtimeSinceStartup - startTime} sec since startup");
        LoadData();
        loadedSaveData = true;
    }

    public void GetAllLevels(LevelSpawnScr levelSpawner)
    {
        levelSpawner.easyLevels = EasyLevels;
        levelSpawner.normalLevels = NormalLevels;
        levelSpawner.possibleHideLevels = HideLevels;
        levelSpawner.safeRoom = SafeRoom;
    }

    private void Update()
    {
        /*
        if (!loadedSaveData)
        {
            print("data not loaded yet");
            if (loadedEnemies && loadedUpgrades && loadedMusicTracks)
            {
                LoadData();
                print("data loaded");
                loadedSaveData = true;
            }
        }
        */
    }
    
    private void OnApplicationQuit()
    {
        SaveData();
    }

    private void SaveData()
    {
        SaveData data = new SaveData();
        PopulateSaveData(data);

        if (!File.Exists(playerDataFilePath + "/data.json"))
        {
            File.CreateText(playerDataFilePath + "/data.json").Close();
        }
        File.WriteAllText(playerDataFilePath + "/data.json", JsonUtility.ToJson(data));
    }

    private void PopulateSaveData(SaveData data)
    {
        data.options.bgMusicVolume = playerSettings.bgMusicVolume;
        data.options.masterVolume = playerSettings.masterVolume;
        data.options.soundFxVolume = playerSettings.soundFXVolume;
        data.options.bgMusicChoice = playerSettings.playerBGMusic;

        data.options.keyBinds.interact = playerSettings.interact;
        data.options.keyBinds.jump = playerSettings.jump;
        data.options.keyBinds.sprint = playerSettings.sprint;
        data.options.keyBinds.crouch = playerSettings.crouch;
        data.options.keyBinds.useItem = playerSettings.useItem;
        data.options.keyBinds.itemSpecial = playerSettings.itemSpecial;
        data.options.useWorldCanvasUI = playerSettings.useWorldCanvasUI;
        
        data.stats.roomsCleared = playerStats.roomsCleared;
        data.stats.chipsEarned = playerStats.chipsEarned;
        data.stats.targetsHit = playerStats.targetsHit;
        data.stats.enemiesKilled = playerStats.enemiesKilled;
        data.stats.runDataList = playerStats.runDataList;
    }

    private void LoadData()
    {
        SaveData data = new SaveData();
        if (deleteDataOnStart)
        {
            File.Delete(playerDataFilePath + "/data.json");
        }
        
        if (File.Exists(playerDataFilePath + "/data.json"))
        {
            try
            {
                data.FromJson(File.ReadAllText(playerDataFilePath + "/data.json"));
            }
            catch (Exception e)
            {
                print(e.Message);
                data = new SaveData();
            }
            
        }
        LoadFromSaveData(data);
    }

    private void LoadFromSaveData(SaveData data)
    {
        print("load data called");
        PlayerController plrController = FindFirstObjectByType<PlayerController>();
        
        playerSettings.bgMusicVolume = data.options.bgMusicVolume;
        playerSettings.masterVolume = data.options.masterVolume;
        playerSettings.soundFXVolume = data.options.soundFxVolume;
        playerSettings.playerBGMusic = data.options.bgMusicChoice != null ?
                data.options.bgMusicChoice : playerSettings.defaultBGMusic.name;
        
        playerSettings.interact = data.options.keyBinds.interact;
        playerSettings.jump = data.options.keyBinds.jump;
        playerSettings.sprint = data.options.keyBinds.sprint;
        playerSettings.crouch = data.options.keyBinds.crouch;
        playerSettings.useItem = data.options.keyBinds.useItem;
        playerSettings.itemSpecial = data.options.keyBinds.itemSpecial;
        playerSettings.useWorldCanvasUI = data.options.useWorldCanvasUI;

        if (plrController != null)
        {
            plrController.interact = data.options.keyBinds.interact;
            plrController.jump = data.options.keyBinds.jump;
            plrController.sprint = data.options.keyBinds.sprint;
            plrController.crouch = data.options.keyBinds.crouch;
            plrController.useItem = data.options.keyBinds.useItem;
            plrController.itemSpecial = data.options.keyBinds.itemSpecial;
        }
        
        playerStats.roomsCleared = data.stats.roomsCleared;
        playerStats.chipsEarned = data.stats.chipsEarned;
        playerStats.targetsHit = data.stats.targetsHit;
        playerStats.enemiesKilled = data.stats.enemiesKilled;
        playerStats.runDataList = data.stats.runDataList;
    }

    private void OnDestroy()
    {
        if (loadedSaveData)
        {
            upgradeOperation.Release();
            enemyOperation.Release();
            musicOperation.Release();
            levelOperation.Release();
        }
    }
 }
