using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class RunDetailsRef : MonoBehaviour
{
    private ApplicationDataManager appDataManager;
    
    public Button exitButton;
    public TMP_Text totalTime;
    public TMP_Text runNumber;
    public TMP_Text deathDoor;
    public TMP_Text chipsEarned;
    public TMP_Text targetsHit;
    
    [Space]
    public ScrollRect enemyScrollRect;
    public ScrollRect upgradeScrollRect;
    public GameObject enemyCountPrefab;
    public GameObject upgradeCountPrefab;

    private void Start()
    {
        appDataManager = FindFirstObjectByType<ApplicationDataManager>();
    }
    
    public void RefreshDetails(RunData runData, int runNum)
    {
        runNumber.text = $"Run #{runNum}";
        totalTime.text = FormatTimeString(runData.totalTime);
        deathDoor.text = $"Died At Room {runData.roomsCleared}";
        chipsEarned.text = $"<sprite name=\"{chipsEarned.spriteAsset.name}\"> <color=\"green\">{runData.chipsEarned}</color> Chips";
        targetsHit.text = $"<sprite name=\"{targetsHit.spriteAsset.name}\"> <color=\"red\">{runData.targetsHit}</color> Targets";

        AddEnemyList(runData);
        AddUpgradeList(runData);
    }
    
    private string FormatTimeString(float timeValue)
    {
        int minutes = (int)timeValue / 60;
        float seconds = timeValue - (minutes * 60);

        string minuteStr = minutes.ToString();
        if (minutes < 10 && minutes >= 0)
            minuteStr = "0" + minuteStr;

        string secondStr = seconds.ToString("F3");
        if (seconds < 10 && seconds >= 0)
            secondStr = "0" + secondStr;

        string millisecondStr = secondStr.Substring(3);
        secondStr = secondStr.Substring(0, 2);

        return minuteStr + ":" + secondStr + "." + millisecondStr;
    }

    private void AddEnemyList(RunData runData)
    {
        foreach (Transform t in enemyScrollRect.content)
        {
            Destroy(t.gameObject);
        }
        
        enemyScrollRect.content.sizeDelta = Vector2.zero;
        
        appDataManager = FindFirstObjectByType<ApplicationDataManager>();
        foreach (EnemyCount count in runData.enemiesKilled)
        {
            EnemyInfo enemyInfo = appDataManager.AllEnemies[count.enemyID];
            RunEnemyCount newCount =
                Instantiate(enemyCountPrefab, enemyScrollRect.content).GetComponent<RunEnemyCount>();
            enemyScrollRect.content.sizeDelta += new Vector2(0, newCount.GetComponent<RectTransform>().sizeDelta.y);
            
            newCount.enemyName.text = enemyInfo.enemyName;
            newCount.count.text = $"{count.count} Killed";
            newCount.icon.sprite = enemyInfo.icon;
        }
        
        enemyScrollRect.verticalScrollbar.value = 1;
    }

    private void AddUpgradeList(RunData runData)
    {
        foreach (Transform t in upgradeScrollRect.content)
        {
            Destroy(t.gameObject);
        }
        
        upgradeScrollRect.content.sizeDelta = Vector2.zero;
        
        appDataManager = FindFirstObjectByType<ApplicationDataManager>();
        foreach (UpgradeCount count in runData.upgradesList)
        {
            UpgradeData upgrade = appDataManager.AllUpgrades[count.upgradeID];
            RunUpgradeCount newCount =
                Instantiate(upgradeCountPrefab, upgradeScrollRect.content).GetComponent<RunUpgradeCount>();
            upgradeScrollRect.content.sizeDelta += new Vector2(0, newCount.GetComponent<RectTransform>().sizeDelta.y);
            
            newCount.upgradeName.text = upgrade.title;
            newCount.count.text = $"(x{count.count})";
            newCount.icon.sprite = upgrade.icon;
        }
        
        upgradeScrollRect.verticalScrollbar.value = 1;
    }
}
