using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ThreatIntroductionSafeRooms
{
    public Threat threat;
    public int safeRoomToIntroduceIn;

    public ThreatIntroductionSafeRooms(ThreatIntroductionSafeRooms old)
    {
        threat = old.threat;
        safeRoomToIntroduceIn = old.safeRoomToIntroduceIn;
    }
}

[System.Serializable]
public class ThreatSpawnerScr : MonoBehaviour
{
    public GameObject infoSheetPickup;
    public List<ThreatIntroductionSafeRooms> threatIntroductionSafeRooms;
    private LevelSpawnScr levelSpawner;
    public List<Threat> threats;
    private Dictionary<Threat, int> threatRoomCountdowns = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelSpawner = FindFirstObjectByType<LevelSpawnScr>();
        foreach (Threat inst in threats)
        {
            threatRoomCountdowns.Add(inst, Random.Range(inst.minRandomRange, inst.maxRandomRange + 1));
            levelSpawner.SpawnInfoSheetForThreat(infoSheetPickup, inst);
        }
    }

    public void TickDownThreats()
    {
        foreach (Threat key in threats)
        {
            threatRoomCountdowns[key]--;
            
            if (threatRoomCountdowns[key] <= 0)
            {
                Instantiate(key.threatPrefab);
                threatRoomCountdowns[key] = Random.Range(key.minRandomRange, key.maxRandomRange + 1);
                if (key.threatName == "Flashout")
                {
                    levelSpawner.spawningHideRoom = true;
                    levelSpawner.roomsBeforeHide = 4;
                }
            }
        }
        
        IntroducePotentialNewThreats(levelSpawner.safeRoomsCleared);
    }

    private void IntroducePotentialNewThreats(int numSafeRoomsPassed)
    {
        List<ThreatIntroductionSafeRooms> introsToRemove = new();
        
        foreach (ThreatIntroductionSafeRooms a in threatIntroductionSafeRooms)
        {
            if (a.safeRoomToIntroduceIn == numSafeRoomsPassed)
            {
                Threat b = a.threat;
                threats.Add(b);
                threatRoomCountdowns.Add(b, Random.Range(b.minRandomRange, b.maxRandomRange + 1));
                levelSpawner.SpawnInfoSheetForThreat(infoSheetPickup, b);
                introsToRemove.Add(a);
            }
        }

        foreach (ThreatIntroductionSafeRooms c in introsToRemove)
        {
            if (threatIntroductionSafeRooms.Contains(c))
            {
                threatIntroductionSafeRooms.Remove(c);
            }
        }
    }
}
