using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class MovementStatsUI : MonoBehaviour
{
    public TMP_Text walljumpText;
    public PlayerController plrController;

    // WallJump Text-Specific Variables
    private int prevWallJumps;
    private float origFontSize = 40;
    private float changeFontSize = 65;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        walljumpText.text = plrController.currWallJumps.ToString();
        prevWallJumps = plrController.currWallJumps;
    }

    // Update is called once per frame
    void Update()
    {
        walljumpText.text = plrController.currWallJumps.ToString();
        if (prevWallJumps != plrController.currWallJumps)
        {
            prevWallJumps = plrController.currWallJumps;
            StopAllCoroutines();
            walljumpText.fontSize = changeFontSize;
            StartCoroutine(SizeTweenWallJumpText(0.3f));
        }
    }

    private IEnumerator SizeTweenWallJumpText(float time)
    {
        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime / time;
            walljumpText.fontSize = Mathf.Lerp(changeFontSize, origFontSize, progress);
            yield return null;
        }
    }
}
