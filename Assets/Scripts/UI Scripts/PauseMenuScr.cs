using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SoundFX))]
public class PauseMenuScr : MonoBehaviour
{
    private readonly UITweenService tweenService = new();
    private bool movingUI;

    public bool optionsMenu;
    public bool upgradesMenu;
    public RectTransform rectTransform;
    
    [Space]
    public GameObject cameraCanvasObject;
    public GameObject[] worldCanvasObjects;
    public GameObject worldRunStatsUI;
    
    [Space]
    public CameraScript cameraScr;

    public OptionsScr optionsScr;
    private PlayerController plrController;
    private AudioSource audioSrc;
    private BGMusic bgMusic;
    private ApplicationDataManager appDataManager;
    public PlayerUpgrades upgradeTracker;

    [Header("Miscellaneous UI References")]
    public Button quitToTitleButton;
    public Button exitButton;

    [Header("Upgrades UI References")]
    public RectTransform upgradesScrollViewContent;
    public GameObject upgradeDescPrefab;
    public Button upgradesButton;

    [Header("Options UI References")]
    public Button optionsButton;

    [Header("Sound FXs")]
    public AudioClip conveyorSFX;
    public AudioClip powerDownSFX;
    public AudioClip buttonConfirmSfx;
    public AudioClip metalCrashSfx;

    private void Awake()
    {
        audioSrc = GetComponent<AudioSource>();
        plrController = GetComponentInParent<PlayerController>();
        bgMusic = FindFirstObjectByType<BGMusic>();
        appDataManager = FindFirstObjectByType<ApplicationDataManager>();
    }

    private void OnEnable()
    {
        audioSrc.Stop();
        audioSrc.PlayOneShot(metalCrashSfx);
        
        bgMusic.MuffleMusic();
        
        optionsMenu = upgradesMenu = movingUI = false;
        transform.localPosition = Vector3.zero;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
        cameraScr.enabled = false;
        
        cameraCanvasObject.SetActive(false);
        worldRunStatsUI.SetActive(false);

        foreach (GameObject obj in worldCanvasObjects)
        {
            obj.SetActive(false);
        }
        optionsScr.RefreshOptions();
        RefreshUpgrades();
        StartCoroutine(tweenService.ShakeEffect(gameObject.GetComponent<RectTransform>(), 10, 0.2f, true));
    }

    private void OnDisable()
    {
        audioSrc.Stop();
        bgMusic.UnMuffleMusic();
        
        optionsMenu = false;
        upgradesMenu = false;

        if (FindFirstObjectByType<DialogueScript>() != null && FindFirstObjectByType<DialogueScript>().playerInDialogue == plrController && FindFirstObjectByType<DialogueScript>().makingChoice)
        {
            Cursor.lockState = CursorLockMode.None;
        } else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        Time.timeScale = 1;
        cameraScr.enabled = true;

        if (plrController.settings.useWorldCanvasUI)
            worldRunStatsUI.SetActive(true);
        else
            cameraCanvasObject.SetActive(true);

        foreach (GameObject obj in worldCanvasObjects)
        {
            obj.SetActive(true);
        }
    }

    private void Update()
    {
        optionsButton.interactable = upgradesButton.interactable = 
            quitToTitleButton.interactable = exitButton.interactable = !(optionsScr.changingBind || movingUI);
    }

    public void QuitToTitleScreen()
    {
        SceneManager.LoadScene("StartScene");
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    private IEnumerator MenuTween(Vector3 deltaPos)
    {
        movingUI = true;

        yield return StartCoroutine(tweenService.TweenPosition(rectTransform, deltaPos, 0.45f, TweenStyle.Quintic, useFixedTime: true));

        movingUI = false;

        audioSrc.Stop();
        audioSrc.PlayOneShot(powerDownSFX);
        StartCoroutine(tweenService.ShakeEffect(gameObject.GetComponent<RectTransform>(), 5, 0.2f, true));
    }

    // ================================= OPTIONS MENU CODE ============================================

    public void Options()
    {
        audioSrc.Stop();
        audioSrc.PlayOneShot(buttonConfirmSfx);
        audioSrc.clip = conveyorSFX;
        audioSrc.loop = true;
        audioSrc.Play();
        if (!optionsMenu)
        {
            StartCoroutine(MenuTween(new Vector3(-2000, 0, 0)));
            optionsMenu = true;
        }
        else
        {
            StartCoroutine(MenuTween(new Vector3(2000, 0, 0)));
            optionsMenu = false;
        }
    }

    // ================================= UPGRADES MENU CODE ===========================================

    public void Upgrades()
    {
        audioSrc.Stop();
        audioSrc.PlayOneShot(buttonConfirmSfx);
        audioSrc.clip = conveyorSFX;
        audioSrc.loop = true;
        audioSrc.Play();
        if (!upgradesMenu)
        {
            StartCoroutine(MenuTween(new Vector3(2000, 0, 0)));
            upgradesMenu = true;
        }
        else
        {
            StartCoroutine(MenuTween(new Vector3(-2000, 0, 0)));
            upgradesMenu = false;
        }
    }

    private void RefreshUpgrades()
    {
        // first, clear the content already in the scroll view
        foreach (Transform t in upgradesScrollViewContent)
        {
            Destroy(t.gameObject);
        }

        foreach (UpgradeCount count in upgradeTracker.upgradeCount)
        {
            UpgradeData data = appDataManager.AllUpgrades[count.upgradeID];
            UpgradeMenuDesc menuDesc = Instantiate(upgradeDescPrefab, upgradesScrollViewContent).GetComponent<UpgradeMenuDesc>();
            menuDesc.title.text = data.title;
            menuDesc.description.text = data.description;
            menuDesc.image.sprite = data.icon;
            menuDesc.count.text = "(x" + count.count + ")";
        }
    }
}
