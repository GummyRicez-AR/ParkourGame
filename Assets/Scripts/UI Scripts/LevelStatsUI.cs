using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;

public class LevelStatsUI : MonoBehaviour
{
    private readonly UITweenService tweenService = new();
    private LevelSpawnScr levelSpawnScr;
    private PlayerController playerController;
    private PlayerSettings settings;
    private int startingLevels;
    private bool dangerUI;

    private LevelStatsBehavior levelStats;
    [Header("Time")]
    public TMP_Text levelTimeUI;
    public TMP_Text levelTimeLabels;
    public TMP_Text bonusLabel;
    public TMP_Text totalTimeUI;
    [Space]
    public TMP_Text worldLevelTimeUI;
    public TMP_Text worldLevelTimeLabels;
    public TMP_Text worldBonusLabel;
    public TMP_Text worldTotalTimeUI;


    [Header("Level")]
    public TMP_Text levelUI;
    [Space]
    public TMP_Text worldLevelUI;

    [Header("Difficulty")]
    public TMP_Text difficultyUI;
    [Space]
    public TMP_Text worldDifficultyUI;

    private int previousLevel;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dangerUI = false;
        levelSpawnScr = FindFirstObjectByType<LevelSpawnScr>();
        levelStats = GetComponentInParent<LevelStatsBehavior>();
        playerController = GetComponentInParent<PlayerController>();
        previousLevel = levelSpawnScr.currLevel;
        settings = playerController.settings;
        
        PauseLevelTime();
        FormatTimeString(totalTimeUI, levelStats.totalTime);
        FormatTimeString(worldTotalTimeUI, levelStats.totalTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (settings.useWorldCanvasUI)
        {
            StartCoroutine(UpdateWorldUI());
        }
        else
        {
            StartCoroutine(UpdatePlayerCanvasUI());
        }
    }

    private IEnumerator UpdatePlayerCanvasUI()
    {
        if (levelSpawnScr != null)
        {
            difficultyUI.text = levelSpawnScr.currDifficulty.ToString("F2");
        }

        if (levelStats.countingLevelTime && levelStats.passedTutorialLevels)
        {
            levelTimeLabels.enabled = true;

            if (!levelStats.inDanger)
            {
                if (dangerUI)
                {
                    dangerUI = false;
                    tweenService.StopShakeElement(levelTimeUI.rectTransform);
                }
                bonusLabel.text = "Bonus";
                levelTimeUI.color = Color.black;
                FormatTimeString(levelTimeUI, levelSpawnScr.runningChipTimeTotal - levelStats.levelTime);
            } else
            {
                if (!dangerUI)
                {
                    dangerUI = true;
                    StartCoroutine(tweenService.ShakeEffect(levelTimeUI.rectTransform, 2, -1));
                }
                bonusLabel.text = "DANGER";
                levelTimeUI.color = Color.red;
                FormatTimeString(levelTimeUI, levelSpawnScr.runningChipTimeTotal - levelStats.levelTime + levelStats.timeLeeway);
            }
        }
        
        FormatTimeString(totalTimeUI, levelStats.totalTime);
        if (levelSpawnScr.currLevel != previousLevel)
        {
            previousLevel = levelSpawnScr.currLevel;
            worldLevelUI.text = levelSpawnScr.currLevel.ToString();
            levelUI.text = levelSpawnScr.currLevel.ToString();

            yield return null;
            if (!settings.useWorldCanvasUI)
            {
                for (int i = 0; i < levelUI.textInfo.characterCount; i++)
                {
                    TMP_CharacterInfo c = levelUI.textInfo.characterInfo[i];
                    StartCoroutine(BounceWordTextOnce(levelUI, 0.2f, 3, 0.1f));
                }
            }
        }

        yield break;
    }

    private IEnumerator UpdateWorldUI()
    {
        if (levelSpawnScr != null)
        {
            worldDifficultyUI.text = levelSpawnScr.currDifficulty.ToString("F2");
        }

        if (levelStats.countingLevelTime && levelStats.passedTutorialLevels)
        {
            worldLevelTimeLabels.enabled = true;

            if (!levelStats.inDanger)
            {
                if (dangerUI)
                {
                    dangerUI = false;
                    tweenService.StopShakeElement(worldLevelTimeUI.rectTransform);
                }
                worldBonusLabel.text = "Bonus";
                worldLevelTimeUI.color = Color.black;
                FormatTimeString(worldLevelTimeUI, levelSpawnScr.runningChipTimeTotal - levelStats.levelTime);
            } else
            {
                if (!dangerUI)
                {
                    dangerUI = true;
                    StartCoroutine(tweenService.ShakeEffect(worldLevelTimeUI.rectTransform, 2, -1));
                }
                bonusLabel.text = "DANGER";
                worldLevelTimeUI.color = Color.red;
                FormatTimeString(worldLevelTimeUI, levelSpawnScr.runningChipTimeTotal - levelStats.levelTime + levelStats.timeLeeway);
            }
        }
        
        FormatTimeString(worldTotalTimeUI, levelStats.totalTime);
        if (levelSpawnScr.currLevel != previousLevel)
        {
            previousLevel = levelSpawnScr.currLevel;
            worldLevelUI.text = levelSpawnScr.currLevel.ToString();
            levelUI.text = levelSpawnScr.currLevel.ToString();

            yield return null;
            if (settings.useWorldCanvasUI)
            {
                for (int i = 0; i < worldLevelUI.textInfo.characterCount; i++)
                {
                    TMP_CharacterInfo c = worldLevelUI.textInfo.characterInfo[i];
                    print(c.character);
                    StartCoroutine(BounceWordTextOnce(worldLevelUI, 0.2f, 5, 0.1f));
                }
            }
        }
    }

    private IEnumerator BounceWordTextOnce(TMP_Text textElement, float time, float bounceStrength, float timeOffsetBetweenCharacters)
    {
        float MAX_PROGRESS = 1 + (timeOffsetBetweenCharacters * textElement.textInfo.characterCount);
        
        float progress = 0;
        while (progress < MAX_PROGRESS)
        {
            textElement.ForceMeshUpdate();
            Mesh mesh = textElement.mesh;
            Vector3[] vertices = mesh.vertices;
            
            progress += Time.deltaTime / time;
            if (progress >= MAX_PROGRESS)
                progress = MAX_PROGRESS;

            for (int i = 0; i < textElement.textInfo.characterCount; i++)
            {
                TMP_CharacterInfo j = textElement.textInfo.characterInfo[i];
                
                int index = j.vertexIndex;
                
                Vector3 offset = new Vector3(0, 
                    bounceStrength * Mathf.Max(0, Mathf.Sin((progress - (timeOffsetBetweenCharacters * i)) * Mathf.PI)), 0);
            
                vertices[index] += offset;
                vertices[index + 1] += offset;
                vertices[index + 2] += offset;
                vertices[index + 3] += offset;
            }
            
            mesh.vertices = vertices;
            textElement.canvasRenderer.SetMesh(mesh);
            yield return null;
        }
    }
    
    private IEnumerator BounceCharacterTextOnce(TMP_Text textElement, float time, TMP_CharacterInfo c, float bounceStrength)
    {
        int index = c.vertexIndex;

        float progress = 0;
        while (progress < 1)
        {
            textElement.ForceMeshUpdate();
            Mesh mesh = textElement.mesh;
            Vector3[] vertices = mesh.vertices;
            
            progress += Time.deltaTime / time;
            if (progress >= 1)
                progress = 1;
            
            Vector3 offset = new Vector3(0, bounceStrength * Mathf.Sin(progress * Mathf.PI), 0);
            
            vertices[index] += offset;
            vertices[index + 1] += offset;
            vertices[index + 2] += offset;
            vertices[index + 3] += offset;
            
            mesh.vertices = vertices;
            textElement.canvasRenderer.SetMesh(mesh);
            yield return null;
        }
    }

    private void FormatTimeString(TMP_Text textUI, float timeValue)
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

        textUI.text = minuteStr + " " + secondStr + " " + millisecondStr;
    }

    public void PauseLevelTime()
    {
        levelTimeUI.text = "PAUSED";
        levelTimeUI.color = Color.blue;
        levelTimeLabels.enabled = false;
        bonusLabel.text = "Bonus";
        
        worldLevelTimeUI.text = "PAUSED";
        worldLevelTimeUI.color = Color.blue;
        worldLevelTimeLabels.enabled = false;
        worldBonusLabel.text = "Bonus";
    }

    private IEnumerator ShakeElement(GameObject element)
    {
        Vector3 origPos = element.transform.localPosition;
        while (levelStats.inDanger)
        {
            Vector3 offset = Random.insideUnitCircle * 2;
            element.transform.localPosition = origPos + offset;
            yield return null;
        }
    }
}
