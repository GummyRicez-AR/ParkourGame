using UnityEngine;
using System.Collections.Generic;

public enum RoomType
{
    None,
    Easy,
    Medium,
    Hard,
    Insane,
    Starting,
    Safe,
    Hide
}

public class LevelTemplateScr : MonoBehaviour
{
    public RoomType roomType;
    
    [Space]
    public bool spawnRandomRoom;
    public bool spawnEnemiesInRoom;
    public bool spawnKeycardOnRandomSpawnpoint;
    public bool spawnKeycardRandomly;
    [Space]
    
    public bool roomCleared;
    public GameObject nextLevelSpawnPoint;
    public GameObject door;
    public GameObject keycardPrefab;
    public GameObject tablePrefab;

    public int roomNumber;
    private InteractOpenDoor doorScr;
    private LevelSpawnScr levelSpawnScr;
    public BoxCollider spawnCollider;
    public float chipTimeThreshold = 4.5f;

    private void Start()
    {
        roomCleared = false;
        levelSpawnScr = FindFirstObjectByType<LevelSpawnScr>();
        doorScr = door.GetComponent<InteractOpenDoor>();

        if (spawnEnemiesInRoom)
        {
            float runningDifficulty = 0;
            Bounds bounds = spawnCollider.bounds;

            while (runningDifficulty < levelSpawnScr.currDifficulty)
            {
                EnemyInfo chosenEnemy = levelSpawnScr.enemyList.infoList[Random.Range(0, levelSpawnScr.enemyList.infoList.Count)];
                float xRange = bounds.extents.x;
                float zRange = bounds.extents.z;
                Vector3 randomPoint = new Vector3(bounds.center.x + Random.Range(-xRange, xRange), bounds.min.y, bounds.center.z + Random.Range(-zRange, zRange));
                Instantiate(chosenEnemy.enemyObject, randomPoint, chosenEnemy.enemyObject.transform.rotation, transform);
                runningDifficulty += chosenEnemy.partDifficulty;
            }
        }

        if (spawnKeycardOnRandomSpawnpoint)
        {
            SpawnKeycardOnRandomSpawnPoint();
        }

        if (spawnKeycardRandomly)
        {
            FindSpawnPointCollider(out Collider spawnCollider);
            Bounds bounds = spawnCollider.bounds;
            
            Vector3 spawnPoint = new Vector3(Random.Range(bounds.min.x, bounds.max.x), bounds.min.y, Random.Range(bounds.min.z, bounds.max.z));
            Instantiate(keycardPrefab, spawnPoint, keycardPrefab.transform.rotation, transform);
        }
    }

    private void Update()
    {
        if (doorScr.doorOpened && !roomCleared)
        {
            roomCleared = true;
            if (spawnRandomRoom)
                levelSpawnScr.SpawnNewLevel();
            else
                levelSpawnScr.IncrementLevel(gameObject);
        }
    }

    public void SpawnKeycardOnRandomSpawnPoint()
    {
        List<Transform> allSpawnPoints = new();
        TraverseHierarchyForSpawnPoints(transform, allSpawnPoints);
        
        Transform chosenSpawn = allSpawnPoints[Random.Range(0, allSpawnPoints.Count)];
        Instantiate(keycardPrefab, chosenSpawn.position, chosenSpawn.rotation, transform);
    }

    private void TraverseHierarchyForSpawnPoints(Transform root, List<Transform> allSpawnPoints)
    {
        foreach (Transform child in root)
        {
            if (child.CompareTag("KeycardSpawnPoint")) {allSpawnPoints.Add(child);}
            
            TraverseHierarchyForSpawnPoints(child, allSpawnPoints);
        }
    }

    private void FindSpawnPointCollider(out Collider resultCollider)
    {
        Collider result = null;
        foreach (Transform t in transform)
        {
            if (t.CompareTag("KeycardSpawnCollider") && t.GetComponent<Collider>() != null)
            {
                result = t.GetComponent<Collider>();
            }
        }

        resultCollider = result;
    }

    public void UnlockSafeDoor()
    {
        doorScr.SafeRoomDoorUnlock();
    }
}
