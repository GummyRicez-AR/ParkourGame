using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public enum TabState
{
    None,
    RunHistory,
    RunDetails,
    TotalStats
}

public class ExtrasMenuScr : MonoBehaviour
{
    private UITweenService tweenService = new();
    
    private TabState currentState;
    private StartSceneScr startSceneScr;

    private ApplicationDataManager appDataManager;
    private PlayerSettings settings;
    private PlayerStats stats;
    
    [Space]
    public GameObject runHistoryPrefab;
    
    public Button runHistoryTab;
    public Button totalStatsTab;
    
    private RectTransform runHistoryTabRT;
    private RectTransform totalStatsTabRT;
    private Vector3 origRunHistoryTabLocalPos;
    private Vector3 origTotalStatsTabLocalPos;
    
    [Header("Content RectTransforms")]
    public RectTransform noRunMessage;
    public RectTransform defaultRect;
    public RectTransform runHistoryRect;
    public RectTransform runDetailsRect;
    public RectTransform totalStatsRect;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private IEnumerator Start()
    {
        startSceneScr = GetComponentInParent<StartSceneScr>();
        appDataManager = FindFirstObjectByType<ApplicationDataManager>();
        settings = appDataManager.playerSettings;
        stats = appDataManager.playerStats;
        
        runHistoryTab.onClick.RemoveAllListeners();
        runHistoryTab.onClick.AddListener(delegate {SwitchTab(TabState.RunHistory); });
        
        totalStatsTab.onClick.RemoveAllListeners();
        totalStatsTab.onClick.AddListener(delegate {SwitchTab(TabState.TotalStats); });
        
        runHistoryTabRT = runHistoryTab.GetComponent<RectTransform>();
        totalStatsTabRT = totalStatsTab.GetComponent<RectTransform>();
        origRunHistoryTabLocalPos = runHistoryTab.GetComponent<RectTransform>().localPosition;
        origTotalStatsTabLocalPos = totalStatsTab.GetComponent<RectTransform>().localPosition;
        
        while (!appDataManager.loadedSaveData)
        {
            yield return null;
        }
        
        SwitchTab(TabState.None, playConfirmSound: false);
        RefreshRunHistory();
    }

    private void SwitchTab(TabState targetState, RunData optionalRunData = null, int runNumber = 0, bool moveTab = true, bool playConfirmSound = true)
    {
        runDetailsRect.gameObject.SetActive(false);
        defaultRect.gameObject.SetActive(false);
        runHistoryRect.gameObject.SetActive(false);
        totalStatsRect.gameObject.SetActive(false);
        
        if (playConfirmSound)
            startSceneScr.PlayConfirmSound();
        
        switch (targetState)
        {
            case TabState.None:
                print("hi");
                defaultRect.gameObject.SetActive(true);
                currentState = TabState.None;
                break;
            case TabState.RunHistory:
                runHistoryRect.gameObject.SetActive(true);
                RefreshRunHistory();

                if (moveTab)
                {
                    if (currentState == TabState.TotalStats)
                    {
                        StartCoroutine(MoveTwoTabs(runHistoryTabRT, totalStatsTabRT));
                    }
                    else if (!(currentState == TabState.RunDetails || currentState == TabState.RunHistory))
                    {
                        StartCoroutine(MoveOneTab(runHistoryTabRT));
                    }
                }
                currentState = TabState.RunHistory;
                break;
            case TabState.RunDetails:
                runDetailsRect.gameObject.SetActive(true);
                runDetailsRect.GetComponent<RunDetailsRef>().RefreshDetails(optionalRunData, runNumber);
                currentState = TabState.RunDetails;
                break;
            case TabState.TotalStats:
                totalStatsRect.gameObject.SetActive(true);
                totalStatsRect.GetComponent<TotalStatsUI>().RefreshTotalStats(stats);

                if (moveTab)
                {
                    if (currentState == TabState.RunDetails || currentState == TabState.RunHistory)
                    {
                        StartCoroutine(MoveTwoTabs(totalStatsTabRT, runHistoryTabRT));
                    }
                    else if (currentState != TabState.TotalStats)
                    {
                        StartCoroutine(MoveOneTab(totalStatsTabRT));
                    }
                }
                currentState = TabState.TotalStats;
                break;
        }
    }

    private IEnumerator MoveOneTab(RectTransform newActiveTab)
    {
        runHistoryTab.interactable = false;
        totalStatsTab.interactable = false;
        
        yield return tweenService.TweenPosition(newActiveTab, new Vector3(-75, 0, 0), 0.5f, TweenStyle.Back);
        
        runHistoryTab.interactable = true;
        totalStatsTab.interactable = true;
    }
    
    private IEnumerator MoveTwoTabs(RectTransform newActiveTab, RectTransform deactiveTab)
    {
        runHistoryTab.interactable = false;
        totalStatsTab.interactable = false;
        
        if (newActiveTab == runHistoryTabRT)
        {
            newActiveTab.localPosition = origRunHistoryTabLocalPos;
        } else if (newActiveTab == totalStatsTabRT)
        {
            newActiveTab.localPosition = origTotalStatsTabLocalPos;
        }

        if (deactiveTab == runHistoryTabRT)
        {
            deactiveTab.localPosition = origRunHistoryTabLocalPos - new Vector3(75, 0, 0);
        } else if (deactiveTab == totalStatsTabRT)
        {
            deactiveTab.localPosition = origTotalStatsTabLocalPos - new Vector3(75, 0, 0);
        }
        
        StartCoroutine(tweenService.TweenPosition(deactiveTab, new Vector3(75, 0, 0), 0.5f, TweenStyle.Back));
        StartCoroutine(tweenService.TweenPosition(newActiveTab, new Vector3(-75, 0, 0), 0.5f, TweenStyle.Back));

        yield return new WaitForSeconds(0.5f);
        
        runHistoryTab.interactable = true;
        totalStatsTab.interactable = true;
    }
    
    private void RefreshRunHistory()
    {
        RectTransform contentArea = runHistoryRect.GetComponentInChildren<ScrollRect>().content;

        if (stats.runDataList.Count <= 0)
        {
            contentArea.gameObject.SetActive(false);
            noRunMessage.gameObject.SetActive(true);
            return;
        }
        
        noRunMessage.gameObject.SetActive(false);
        contentArea.gameObject.SetActive(true);
        foreach (RectTransform rtf in contentArea)
        {
            Destroy(rtf.gameObject);
        }
        
        contentArea.sizeDelta = new Vector2(0, 0);
        
        foreach (RunData run in stats.runDataList)
        {
            RunHistoryRef newRef = Instantiate(runHistoryPrefab, contentArea).GetComponent<RunHistoryRef>();
            contentArea.sizeDelta += new Vector2(0, contentArea.GetComponent<GridLayoutGroup>().cellSize.y);
            
            newRef.runNumber.text = $"Run #{stats.runDataList.IndexOf(run) + 1}";
            newRef.deathDoor.text = $"Died At Room {run.roomsCleared}";

            newRef.GetComponent<Button>().onClick.RemoveAllListeners();
            newRef.GetComponent<Button>().onClick
                .AddListener(delegate {SwitchTab(TabState.RunDetails, optionalRunData: run, runNumber: stats.runDataList.IndexOf(run) + 1, moveTab: false); });
        }
        
        runDetailsRect.GetComponent<RunDetailsRef>().exitButton.onClick.RemoveAllListeners();
        runDetailsRect.GetComponent<RunDetailsRef>().exitButton.onClick.AddListener(delegate {SwitchTab(TabState.RunHistory, moveTab: false); });
    }
}
