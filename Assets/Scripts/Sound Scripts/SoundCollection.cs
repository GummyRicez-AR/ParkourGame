using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundCollection: MonoBehaviour
{
    private ApplicationDataManager manager;
    public bool playOnAwake;
    public List<SoundFX> soundFXes = new();
    public BGMusic BGMusic;
    public PlayerSettings settings;

    private IEnumerator Start()
    {
        yield return null;
        manager = FindFirstObjectByType<ApplicationDataManager>();
        while (!manager.loadedSaveData)
        {
            yield return null;
        }

        BGMusic.GetSource().clip = manager.AllMusicTracks[settings.playerBGMusic];
        BGMusic.volumeMult = (settings.bgMusicVolume / 100) * (settings.masterVolume / 100);
        
        if (playOnAwake)
            BGMusic.GetSource().Play();
    }
}
