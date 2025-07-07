using UnityEngine;
using UnityEngine.Audio;

public class BGMusic : SoundFX
{
    public AudioMixerGroup mixer;
    private ApplicationDataManager dataManager;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        dataManager = FindFirstObjectByType<ApplicationDataManager>();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        source = GetComponent<AudioSource>();
        soundCollection = FindFirstObjectByType<SoundCollection>();
        soundCollection.BGMusic = this;
    }

    public void MuffleMusic()
    {
        source.outputAudioMixerGroup = mixer;
    }

    public void UnMuffleMusic()
    {
        source.outputAudioMixerGroup = null;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
