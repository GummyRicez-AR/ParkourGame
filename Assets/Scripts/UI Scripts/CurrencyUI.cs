using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class CurrencyUI : MonoBehaviour
{
    public InventoryScript inventory;
    
    [Header("Currencies")]
    public TMP_Text chipText;

    public TMP_Text worldChipText;
    
    [Space]
    private int oldChips;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        oldChips = inventory.chips;
    }

    // Update is called once per frame
    void Update()
    {
        if (oldChips != inventory.chips)
        {
            oldChips = inventory.chips;
            StopAllCoroutines();
            StartCoroutine(TweenFloatValue(chipText, inventory.chips, 1));
            StartCoroutine(TweenFloatValue(worldChipText, inventory.chips, 1));
        }
    }

    private IEnumerator TweenFloatValue(TMP_Text textUI, float target, float tweenTime)
    {
        float orig = float.Parse(textUI.text);
        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime / tweenTime;
            textUI.text = ((int)Mathf.Lerp(orig, target, -Mathf.Pow(progress, 2) + (2 * progress))).ToString();
            yield return null;
        }

        StartCoroutine(BounceWordTextOnce(textUI, 0.5f, 5, 0.15f));
    }
    
    private IEnumerator BounceWordTextOnce(TMP_Text textElement, float time, float bounceStrength, float timeOffsetBetweenCharacters)
    {
        textElement.fontWeight += 800;
        float MAX_PROGRESS = 1 + (timeOffsetBetweenCharacters * textElement.textInfo.characterCount);
        
        float progress = 0;
        while (progress < MAX_PROGRESS)
        {
            textElement.ForceMeshUpdate();
            Mesh mesh = textElement.mesh;
            Vector3[] vertices;
            try
            {
                vertices = mesh.vertices;
            } catch
            {
                yield break;
            }
            
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
        
        textElement.fontWeight -= 800;
    }
}
