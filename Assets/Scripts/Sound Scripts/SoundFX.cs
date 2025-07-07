using UnityEngine;

[System.Serializable]
[RequireComponent(typeof(AudioSource))]
public class SoundFX : MonoBehaviour
{
    protected AudioSource source;
    protected SoundCollection soundCollection;
    public float volumeMult = 1;
    public bool volumeHandledByOtherScript;

    protected virtual void Start()
    {
        source = GetComponent<AudioSource>();
        soundCollection = FindFirstObjectByType<SoundCollection>();
        soundCollection.soundFXes.Add(this);

        volumeMult = (soundCollection.settings.soundFXVolume / 100) * (soundCollection.settings.masterVolume / 100);
    }

    protected virtual void Update()
    {
        if (!volumeHandledByOtherScript)
        {
            source.volume = volumeMult;
        }
    }

    protected virtual void OnDestroy()
    {
        if (soundCollection != null)
            soundCollection.soundFXes.Remove(this);
    }

    public AudioSource GetSource() { return source; }
}
