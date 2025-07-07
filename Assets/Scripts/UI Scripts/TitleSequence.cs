using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

public class TitleSequence : MonoBehaviour
{
    private readonly UITweenService tweenService = new();
    private readonly string allChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_";
    private StartSceneScr startSceneScr;
    private ApplicationDataManager dataManager;

    [Header("Text Labels")]
    public TMP_Text upgradesLoaded;
    public TMP_Text enemiesLoaded;
    public TMP_Text musicLoaded;
    public TMP_Text levelsLoaded;
    
    public TMP_Text waitingForInputText;

    private void Start()
    {
        startSceneScr = FindFirstObjectByType<StartSceneScr>();
        dataManager = FindFirstObjectByType<ApplicationDataManager>();
        StartCoroutine(TextRandomBlinkEffect(upgradesLoaded));
        StartCoroutine(TextRandomBlinkEffect(enemiesLoaded));
        StartCoroutine(TextRandomBlinkEffect(musicLoaded));
        StartCoroutine(TextRandomBlinkEffect(levelsLoaded));
        StartCoroutine(TextRandomBlinkEffect(waitingForInputText));
    }

    private void Update()
    {
        /*
        if (dataManager.upgradeOperation.Status == AsyncOperationStatus.Succeeded)
            upgradesLoaded.text = "upgrades: " + dataManager.upgradesLoaded + " (DONE)";
        else
            upgradesLoaded.text = "upgrades: " + dataManager.upgradesLoaded;
        
        if (dataManager.enemyOperation.Status == AsyncOperationStatus.Succeeded)
            enemiesLoaded.text = "enemies: " + dataManager.enemiesLoaded + " (DONE)";
        else
            enemiesLoaded.text = "enemies: " + dataManager.enemiesLoaded;
        
        if (dataManager.musicOperation.Status == AsyncOperationStatus.Succeeded)
            musicLoaded.text = "music: " + dataManager.musicLoaded + " (DONE)";
        else
            musicLoaded.text = "music: " + dataManager.musicLoaded;
        
        if (dataManager.levelOperation.Status == AsyncOperationStatus.Succeeded)
            levelsLoaded.text = "levels: " + dataManager.levelsLoaded + " (DONE)";
        else
            levelsLoaded.text = "levels: " + dataManager.levelsLoaded;
        */

        if (!dataManager.loadedSaveData)
        {
            if (dataManager.upgradesLoaded <= 0)
                upgradesLoaded.gameObject.SetActive(false);
            else
            {
                upgradesLoaded.gameObject.SetActive(true);
                upgradesLoaded.text = "upgrades: " + dataManager.upgradesLoaded;
            }

            if (dataManager.enemiesLoaded <= 0)
                enemiesLoaded.gameObject.SetActive(false);
            else
            {
                enemiesLoaded.gameObject.SetActive(true);
                enemiesLoaded.text = "enemies: " + dataManager.enemiesLoaded;
            }

            if (dataManager.musicLoaded <= 0)
                musicLoaded.gameObject.SetActive(false);
            else
            {
                musicLoaded.gameObject.SetActive(true);
                musicLoaded.text = "music: " + dataManager.musicLoaded;
            }

            if (dataManager.levelsLoaded <= 0)
                levelsLoaded.gameObject.SetActive(false);
            else
            {
                levelsLoaded.gameObject.SetActive(true);
                levelsLoaded.text = "levels: " + dataManager.levelsLoaded;
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public IEnumerator ListenForKey()
    {
        while (!Input.anyKeyDown)
        {
            yield return null;
        }
        StopAllCoroutines();
        
        StartCoroutine(TurnOffText(upgradesLoaded));
        StartCoroutine(TurnOffText(enemiesLoaded));
        StartCoroutine(TurnOffText(musicLoaded));
        StartCoroutine(TurnOffText(levelsLoaded));
        StartCoroutine(TurnOffText(waitingForInputText));
        
        startSceneScr.PlayConfirmSound();
        yield return new WaitForSecondsRealtime(1);
        Destroy(gameObject);
    }

    private IEnumerator TextRandomBlinkEffect(TMP_Text textElement)
    {
        Color origColor = textElement.color;
        while (gameObject.activeInHierarchy)
        {
            print("thing");
            
            textElement.ForceMeshUpdate(ignoreActiveState: true);
            Mesh tMesh = textElement.mesh;
            Vector3[] vertices = tMesh.vertices;

            for (int i = 0; i < textElement.textInfo.characterCount; i++)
            {
                TMP_CharacterInfo c = textElement.textInfo.characterInfo[i];
                
                int index = c.vertexIndex;
                Color32[] materialColors = textElement.textInfo.meshInfo[c.materialReferenceIndex].colors32;

                if (Random.Range(0f, 1f) > 0.95f && c.character != ' ')
                {
                    c.character = allChars[Random.Range(0, allChars.Length)];
                    float xDist = Mathf.Abs(vertices[index + 2].x - vertices[index + 1].x);
                    float yDist = Mathf.Abs(vertices[index + 1].y - vertices[index].y);

                    Vector3 xOffset = new Vector3(xDist / 18f, 0, 0);
                    Vector3 yOffset = new Vector3(0, yDist / 18f, 0);
                    
                    vertices[index] += xOffset + yOffset; // bottom left
                    vertices[index + 1] += xOffset - yOffset; // top left
                    vertices[index + 2] += -xOffset - yOffset; // top right
                    vertices[index + 3] += -xOffset + yOffset; // bottom right

                    Color blinkColor = (Color)c.color * 0.9f;
                    materialColors[index] = blinkColor;
                    materialColors[index + 1] = blinkColor;
                    materialColors[index + 2] = blinkColor;
                    materialColors[index + 3] = blinkColor;
                    
                    /*
                    for (int j = i+1; j < textElement.textInfo.characterCount; j++)
                    {
                        TMP_CharacterInfo c2 = textElement.textInfo.characterInfo[j];
                        int index2 = c2.vertexIndex;

                        vertices[index2] += -xOffset;
                        vertices[index2 + 1] += -xOffset;
                        vertices[index2 + 2] += -xOffset;
                        vertices[index2 + 3] += -xOffset;
                    }
                    */
                }
                else
                {
                    materialColors[index] = origColor;
                    materialColors[index + 1] = origColor;
                    materialColors[index + 2] = origColor;
                    materialColors[index + 3] = origColor;
                }
            }
            
            tMesh.vertices = vertices;
            textElement.canvasRenderer.SetMesh(tMesh);
            textElement.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            yield return new WaitForSecondsRealtime(0.03f);
        }
    }

    private IEnumerator TurnOffText(TMP_Text textElement)
    {
        const float TICK_TIME = 0.045f;
        while (textElement.color.a > 0)
        {
            textElement.color -= new Color(0, 0, 0, TICK_TIME);
            SwitchRandomCharacters(textElement);
            yield return new WaitForSecondsRealtime(TICK_TIME);
        }
    }
    
    private void SwitchRandomCharacters(TMP_Text textElement)
    {
        string origStr = textElement.text;
        string newStr = "";
        
        for (int i = 0; i < origStr.Length; i++)
        {
            newStr += allChars[Random.Range(0, allChars.Length)];
        }
        
        textElement.text = newStr;
    }
}
