using UnityEngine;
using TMPro;

public class TotalStatsUI : MonoBehaviour
{
    public TMP_Text totalRuns;
    public TMP_Text totalTime;
    public TMP_Text totalChips;
    public TMP_Text targetsHit;

    public void RefreshTotalStats(PlayerStats stats)
    {
        totalRuns.text = $"Attempted <color=\"yellow\">{stats.runDataList.Count}</color> runs";

        float totalTimeVal = 0;
        foreach (RunData d in stats.runDataList)
        {
            totalTimeVal += d.totalTime;
        }
        totalTime.text = $"Ran for a total of <color=\"yellow\">{FormatLongTimeString(totalTimeVal)}</color>";

        totalChips.text = $"Collected <sprite name=\"{totalChips.spriteAsset.name}\"> <color=\"green\">{stats.chipsEarned}</color> chips";
        targetsHit.text =
            $"Hit <sprite name=\"{targetsHit.spriteAsset.name}\"> <color=\"red\">{stats.targetsHit}</color> targets";
    }
    
    private string FormatLongTimeString(float timeValue)
    {
        int hours = (int)timeValue / 3600;
        int minutes = ((int)timeValue / 60) - (hours * 60);
        float seconds = timeValue - (minutes * 60);
        
        string hourStr = hours.ToString();
        if (hours < 10 && hours >= 0)
            hourStr = "0" + hourStr;
        
        string minuteStr = minutes.ToString();
        if (minutes < 10 && minutes >= 0)
            minuteStr = "0" + minuteStr;

        string secondStr = seconds.ToString("F3");
        if (seconds < 10 && seconds >= 0)
            secondStr = "0" + secondStr;

        string millisecondStr = secondStr.Substring(3);
        secondStr = secondStr.Substring(0, 2);

        if (hours < 1)
        {
            return minuteStr + ":" + secondStr + "." + millisecondStr;
        }
        return hourStr + ":" + minuteStr + ":" + secondStr + "." + millisecondStr;
    }
}
