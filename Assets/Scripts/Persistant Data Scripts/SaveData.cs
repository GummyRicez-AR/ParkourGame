using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemyCount
{
    public string enemyID;
    public int count;

    public EnemyCount(EnemyCount old)
    {
        enemyID = old.enemyID;
        count = old.count;
    }

    public EnemyCount(string newID)
    {
        enemyID = newID;
        count = 1;
    }
}

[System.Serializable]
public class UpgradeCount
{
    public string upgradeID;
    public int count;
}

[System.Serializable]
public class RunData
{
    public float totalTime;
    public int roomsCleared;
    public int chipsEarned;
    public List<EnemyCount> enemiesKilled = new();
    public int targetsHit;
    public List<UpgradeCount> upgradesList = new();
}

[System.Serializable]
public class SaveData
{
    [System.Serializable]
    public class Options
    {
        [System.Serializable]
        public class KeyBinds
        {
            public KeyCode interact = KeyCode.E;
            public KeyCode useItem = KeyCode.Mouse0;
            public KeyCode itemSpecial = KeyCode.Q;
            public KeyCode jump = KeyCode.Space;
            public KeyCode crouch = KeyCode.Mouse1;
            public KeyCode sprint = KeyCode.LeftShift;
        }
        
        public float masterVolume = 100;
        public float soundFxVolume = 100;
        public float bgMusicVolume = 100;

        [SerializeField] public string bgMusicChoice;
        
        public KeyBinds keyBinds = new KeyBinds();
        public bool useWorldCanvasUI;
    }

    [System.Serializable]
    public class Stats
    {
        public int roomsCleared;
        public int chipsEarned;
        public List<EnemyCount> enemiesKilled = new();
        public int targetsHit;

        public List<RunData> runDataList = new();
    }
    
    public Options options = new();
    public Stats stats = new();

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void FromJson(string jsonString)
    {
        JsonUtility.FromJsonOverwrite(jsonString, this);
    }
}
