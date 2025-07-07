using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SoundFX))]
public class StartSceneScr : MonoBehaviour
{
    private static bool _firstTimeInScene = false;
    private AsyncOperation asyncLoad;
    private readonly UITweenService tweenService = new();
    private ApplicationDataManager dataManager;
    private AudioSource audioSrc;
    private OptionsScr optionsScr;
    private ExtrasMenuScr extrasMenuScr;
    private bool movingUI;

    [Header("UI References")]
    public GameObject titleSequenceObj;
    public RectTransform foregroundCollection;
    public Image background;
    public Image logo;
    public Image blackBG;
    public Button optionsButton;
    public Button extrasButton;

    [Header("Sound Clips")]
    public AudioClip conveyorSfx;
    public AudioClip powerDownSfx;
    public AudioClip buttonConfirmSfx;

    [Space]
    public bool inOptions;
    public bool inExtras;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        dataManager = FindFirstObjectByType<ApplicationDataManager>();
        Cursor.lockState = CursorLockMode.None;
        movingUI = inOptions = inExtras = false;
        optionsScr = GetComponentInChildren<OptionsScr>();
        extrasMenuScr =  GetComponentInChildren<ExtrasMenuScr>();
        audioSrc = GetComponent<AudioSource>();
        logo.rectTransform.sizeDelta = new Vector2(0, 0);
        logo.rectTransform.localPosition = new Vector3(-2000, 250, 0);
        logo.color = Color.black;
        
        optionsButton.onClick.RemoveAllListeners();
        optionsButton.onClick.AddListener(Options);
        extrasButton.onClick.RemoveAllListeners();
        extrasButton.onClick.AddListener(Extras);

        StartCoroutine(BgMoveCycle(736, 100));
        optionsScr.RefreshOptions();
        asyncLoad = SceneManager.LoadSceneAsync("GameScene");
        asyncLoad.allowSceneActivation = false;
        
        if (!_firstTimeInScene)
        {
            _firstTimeInScene = true;
            StartCoroutine(TitleSequenceRoutine());
        }
        else
        {
            Destroy(titleSequenceObj);
        }

        blackBG.enabled = false;
    }

    private void Update()
    {
        optionsButton.interactable = optionsScr.changingBind || movingUI ? false : true;
        extrasButton.interactable = optionsScr.changingBind || movingUI ? false : true;
    }

    // left movement, snaps to the right once out of range
    private IEnumerator BgMoveCycle(float outwardXRange, float speed)
    {
        while (true)
        {
            background.rectTransform.Translate(speed * Time.deltaTime * Vector3.left);
            if (background.rectTransform.localPosition.x < -Mathf.Abs(outwardXRange))
            {
                background.rectTransform.localPosition = new Vector3(outwardXRange, 0, 0);
            }
            yield return null;
        }
    }
    
    public void PlayConfirmSound() {audioSrc.PlayOneShot(buttonConfirmSfx);}

    private IEnumerator TitleSequenceRoutine()
    {
        titleSequenceObj.SetActive(true);
        TitleSequence seq = titleSequenceObj.GetComponent<TitleSequence>();
        seq.waitingForInputText.enabled = false;
        while (!dataManager.loadedSaveData)
        {
            yield return null;
        }

        seq.waitingForInputText.enabled = true;
        yield return StartCoroutine(seq.ListenForKey());
        
        StartCoroutine(tweenService.TweenPosition(logo.rectTransform, new Vector3(2000, 0, 0), 1, TweenStyle.Back));
        StartCoroutine(tweenService.TweenSize(logo.rectTransform, 1000, 600, 1, TweenStyle.Quadratic));
        StartCoroutine(tweenService.TweenImageColor(logo, Color.white, 1, TweenStyle.Quintic));
        FindFirstObjectByType<BGMusic>().GetSource().Play();
    }

    public async void BeginGame(Button buttonPressed)
    {
        buttonPressed.interactable = false;
        buttonPressed.GetComponentInChildren<TMPro.TMP_Text>().text = "PLAYING";

        blackBG.enabled = true;
        blackBG.color = new Color(0, 0, 0, 0);
        while (blackBG.color.a < 1)
        {
            await Task.Yield();
            blackBG.color += new Color(0, 0, 0, 1 * Time.deltaTime);
        }
        
        asyncLoad.allowSceneActivation = true;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // ====================================== OPTIONS CODE ===========================================

    private void Options()
    {
        audioSrc.Stop();
        PlayConfirmSound();
        audioSrc.clip = conveyorSfx;
        audioSrc.loop = true;
        audioSrc.Play();
        
        if (!inOptions)
        {
            foregroundCollection.localPosition = Vector3.zero;
            inOptions = true;
            StartCoroutine(MenuTween(new Vector3(-2000, 0, 0)));
        } else
        {
            foregroundCollection.localPosition = new Vector3(-2000, 0, 0);
            inOptions = false;
            StartCoroutine(MenuTween(new Vector3(2000, 0, 0)));
        }
    }

    private IEnumerator MenuTween(Vector3 deltaPos)
    {
        movingUI = true;
        yield return StartCoroutine(tweenService.TweenPosition(foregroundCollection, deltaPos, 0.6f, TweenStyle.Quintic));
        movingUI = false;
        audioSrc.Stop();
        audioSrc.PlayOneShot(powerDownSfx);
    }
    
    // =================================== EXTRAS CODE =====================================

    private void Extras()
    {
        audioSrc.Stop();
        PlayConfirmSound(); 
        audioSrc.clip = conveyorSfx;
        audioSrc.loop = true;
        audioSrc.Play();

        if (!inExtras)
        {
            foregroundCollection.localPosition = Vector3.zero;
            StartCoroutine(MenuTween(new Vector3(2000, 0, 0)));
            inExtras = true;
        }
        else
        {
            foregroundCollection.localPosition = new Vector3(2000, 0, 0);
            StartCoroutine(MenuTween(new Vector3(-2000, 0, 0)));
            inExtras = false;
        }
    }
}
