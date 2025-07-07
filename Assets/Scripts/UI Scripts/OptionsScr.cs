using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(SoundFX))]
public class OptionsScr : MonoBehaviour
{
    public bool changingBind;
    public PlayerSettings settings;
    public List<AudioClip> selectableBGMusicChoices = new();
    
    private SoundCollection soundCollection;
    private AudioSource audioSource;
    private ApplicationDataManager dataManager;

    [Header("Audio Clips")] public AudioClip confirmSFX;

    [Header("UI References")] public Image bgPanel;
    public Slider masterVolumeSlider;
    public TMP_Text masterVolumeText;
    public Slider soundFXSlider;
    public TMP_Text soundFXText;
    public Slider bgMusicSlider;
    public TMP_Text bgMusicText;

    [Space]
    public TMP_Dropdown bgMusicDropdown;

    public Toggle worldCanvasToggle;

    [Space]
    public RectTransform interactKeybind;
    public RectTransform itemSpecialKeybind;
    public RectTransform sprintKeybind;
    public RectTransform crouchKeyBind;

    
    private void Start()
    {
        soundCollection = FindFirstObjectByType<SoundCollection>();
        audioSource = GetComponent<AudioSource>();
        changingBind = false;
    }

    private void Update()
    {
        bgPanel.rectTransform.sizeDelta = new Vector2(0, 0);
    }

    public void RefreshOptions()
    {
        bgMusicDropdown.onValueChanged.RemoveAllListeners();
        
        UpdateSliderValue(soundFXSlider, soundFXText);
        UpdateSliderValue(masterVolumeSlider, masterVolumeText);
        UpdateSliderValue(bgMusicSlider, bgMusicText);

        masterVolumeSlider.value = settings.masterVolume;
        bgMusicSlider.value = settings.bgMusicVolume;
        soundFXSlider.value = settings.soundFXVolume;

        bgMusicDropdown.ClearOptions();
        List<string> newOptions = new();
        foreach (AudioClip clip in selectableBGMusicChoices)
        {
            newOptions.Add(clip.name);
        }
        bgMusicDropdown.AddOptions(newOptions);
        
        bgMusicDropdown.onValueChanged.RemoveAllListeners();

        for (int i = 0; i < bgMusicDropdown.options.Count; i++)
        {
            if (settings.playerBGMusic == bgMusicDropdown.options[i].text)
            {
                bgMusicDropdown.value = i;
                UpdateBGMusic();
                break;
            }
        }
        
        bgMusicDropdown.onValueChanged.AddListener(delegate { UpdateBGMusic(); });
        
        worldCanvasToggle.onValueChanged.RemoveAllListeners();
        worldCanvasToggle.isOn = settings.useWorldCanvasUI;
        worldCanvasToggle.onValueChanged.AddListener(delegate {ToggleWorldCanvas();});
        
        AssignKeybindListeners(interactKeybind);
        AssignKeybindListeners(itemSpecialKeybind);
        AssignKeybindListeners(sprintKeybind);
        AssignKeybindListeners(crouchKeyBind);
    }

    private void AssignKeybindListeners(RectTransform t)
    {
        Button button =  t.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { StartCoroutine(KeybindChange(t));});
        

        switch (t.name)
        {
            case "Interact":
                button.GetComponentInChildren<TMP_Text>().text = settings.interact.ToString();
                break;
            case "ItemSpecial":
                button.GetComponentInChildren<TMP_Text>().text = settings.itemSpecial.ToString();
                break;
            case "Sprint":
                button.GetComponentInChildren<TMP_Text>().text = settings.sprint.ToString();
                break;
            case "Crouch":
                button.GetComponentInChildren<TMP_Text>().text = settings.crouch.ToString();
                break;
        }
    }

    private void CheckSoundCollection()
    {
        if (soundCollection == null)
        {
            soundCollection = FindFirstObjectByType<SoundCollection>();
        }
    }

    private void UpdateSliderValue(Slider slider, TMP_Text textUI)
    {
        textUI.text = Mathf.Round(slider.value).ToString();
    }

    public void MasterVolumeSliderUpdate()
    {
        UpdateSliderValue(masterVolumeSlider, masterVolumeText);
        CheckSoundCollection();
        foreach (SoundFX sound in soundCollection.soundFXes)
        {
            if (!sound.volumeHandledByOtherScript)
            {
                sound.volumeMult = (soundFXSlider.value / 100f) * (masterVolumeSlider.value / 100f);
            }
        }
        soundCollection.BGMusic.volumeMult = (bgMusicSlider.value / 100f) * (masterVolumeSlider.value / 100f);
        settings.masterVolume = masterVolumeSlider.value;
    }

    public void SoundFXSliderUpdate()
    {
        UpdateSliderValue(soundFXSlider, soundFXText);
        CheckSoundCollection();
        foreach (SoundFX sound in soundCollection.soundFXes)
        {
            if (!sound.volumeHandledByOtherScript)
            {
                sound.volumeMult = (soundFXSlider.value / 100f) * (masterVolumeSlider.value / 100f);
            }
        }
        settings.soundFXVolume = soundFXSlider.value;
    }

    public void BGMusicSliderUpdate()
    {
        UpdateSliderValue(bgMusicSlider, bgMusicText);
        CheckSoundCollection();
        soundCollection.BGMusic.volumeMult = (bgMusicSlider.value / 100f) * (masterVolumeSlider.value / 100f);
        settings.bgMusicVolume = bgMusicSlider.value;
    }

    private void UpdateBGMusic()
    {
        AudioClip selectedClip = null;
        foreach (AudioClip clip in selectableBGMusicChoices)
        {
            if (clip.name == bgMusicDropdown.options[bgMusicDropdown.value].text)
            {
                selectedClip = clip;
                break;
            }
        }

        if (selectedClip != null && selectedClip.name != settings.playerBGMusic)
        {
            soundCollection.BGMusic.GetSource().Stop();
            soundCollection.BGMusic.GetSource().clip = selectedClip;
            
            settings.playerBGMusic = selectedClip.name;
            soundCollection.BGMusic.GetSource().Play();
        }
    }

    private void ToggleWorldCanvas()
    {
        settings.useWorldCanvasUI = worldCanvasToggle.isOn;
    }

    private IEnumerator KeybindChange(RectTransform keybindRect)
    {
        PlayerController plrController = FindFirstObjectByType<PlayerController>();
        
        Button button = keybindRect.GetComponentInChildren<Button>();
        button.interactable = false;
        button.GetComponentInChildren<TMP_Text>().text = "waiting for keybind (BACKSPACE to cancel)";

        bool keybindConfirmed = false;
        while (!keybindConfirmed)
        {
            yield return WaitUntilKeyPressed();

            audioSource.PlayOneShot(confirmSFX);
            KeyCode keyPressed = KeyCode.None;
            foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(k))
                {
                    keyPressed = k;
                    break;
                }
            }
            
            print("key pressed: " + keyPressed);

            if (keyPressed != KeyCode.Backspace)
            {
                switch (keybindRect.name)
                {
                    case "Interact":
                        if (!(keyPressed == settings.crouch || keyPressed == settings.sprint || keyPressed == settings.crouch
                            || keyPressed == settings.itemSpecial))
                        {
                            settings.interact = keyPressed;
                            keybindConfirmed = true;
                        }
                        break;
                        
                    case "ItemSpecial":
                        if (!(keyPressed == settings.crouch || keyPressed == settings.sprint || keyPressed == settings.crouch
                              || keyPressed == settings.interact))
                        {
                            settings.itemSpecial = keyPressed;
                            keybindConfirmed = true;
                        }
                        break;
                    
                    case "Sprint":
                        if (!(keyPressed == settings.crouch || keyPressed == settings.interact || keyPressed == settings.crouch
                              || keyPressed == settings.itemSpecial))
                        {
                            settings.sprint = keyPressed;
                            keybindConfirmed = true;
                        }
                        break;
                    
                    case "Crouch":
                        if (!(keyPressed == settings.interact || keyPressed == settings.sprint || keyPressed == settings.crouch
                              || keyPressed == settings.itemSpecial))
                        {
                            settings.interact = keyPressed;
                            keybindConfirmed = true;
                        }
                        break;
                }
            }
            else
            {
                switch (keybindRect.name)
                {
                    case "Interact":
                        button.GetComponentInChildren<TMP_Text>().text = settings.interact.ToString();
                        break;
                    case "ItemSpecial":
                        button.GetComponentInChildren<TMP_Text>().text = settings.itemSpecial.ToString();
                        break;
                    case "Sprint":
                        button.GetComponentInChildren<TMP_Text>().text = settings.sprint.ToString();
                        break;
                    case "Crouch":
                        button.GetComponentInChildren<TMP_Text>().text = settings.crouch.ToString();
                        break;
                }
            }

            if (keybindConfirmed)
            {
                audioSource.PlayOneShot(confirmSFX);
                button.interactable = true;
                changingBind = false;
                if (!Input.GetKeyDown(KeyCode.Backspace))
                {
                    button.GetComponentInChildren<TMP_Text>().text = keyPressed.ToString();
                }

                if (plrController is not null)
                {
                    plrController.RefreshKeyBindControls();
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator WaitUntilKeyPressed()
    {
        changingBind = true;
        while (!Input.anyKeyDown)
        {
            yield return null;
        }
    }
}
