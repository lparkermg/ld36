using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Helpers.Filesystem;
using Entities;
public class GameManager : Singleton<GameManager> {
    protected GameManager() { }

    //Setting Variables Stuff... (May change to a single class.)
    public float MusicVolume { get; private set; }
    public float SfxVolume { get; private set; }

    public List<AudioClip> BackgroundClips { get; private set; }
    public List<AudioClip> HitSfxClips { get; private set; }
    public AudioClip BattleWinSfxClip { get; private set; }
    public AudioClip BattleLostSfxClip { get; private set; }
    public AudioClip RoundWinClip { get; private set; }
    public AudioClip RoundLostClip { get; private set; }
    public AudioClip RoundDrawClip { get; private set; }

    public bool IsDirty { get; private set; }

    public void Initialise()
    {
        LoadSettings();
    }
    
    public void StartGame()
    {
        
        LoadScene(2);
    }

    #region Setting IO etc.
    public void LoadSettings()
    {
        var loadedSettings = FileIo.LoadSettingsOrDefault();
        MusicVolume = loadedSettings.MusicVolume;
        SfxVolume = loadedSettings.SfxVolume;
        IsDirty = false;
    }

    public void SaveSettings()
    {
        var settings = new Settings(MusicVolume,SfxVolume);
        FileIo.SaveSettings(settings);
        IsDirty = false;
    }

    public void UpdateMusicVolume(float newVol)
    {
        MusicVolume = newVol;
        IsDirty = true;
    }

    public void UpdateSfxVolume(float newVol)
    {
        SfxVolume = newVol;
        IsDirty = true;
    }
    #endregion

    #region Audio Loading
    public void PopulateBgmClips(List<AudioClip> audio)
    {
        BackgroundClips = audio;
    }
    //, AudioClip battleWonSfx, AudioClip battleLostSfx, AudioClip roundWonSfx, AudioClip roundLostSfx, AudioClip roundDrawnSfx
    public void PopulateSfxClips(List<AudioClip> hitSfx)
    {
        HitSfxClips = hitSfx;
        //BattleWinSfxClip = battleWonSfx;
        //BattleLostSfxClip = battleLostSfx;
        //RoundWinClip = roundWonSfx;
        //RoundLostClip = roundLostSfx;
        //RoundDrawClip = roundDrawnSfx;
    }

    public AudioClip GetRandomAudioClip()
    {
        var clip = Random.Range(0, BackgroundClips.Count);
        return BackgroundClips[clip];
    }

    public AudioClip GetRandomHitSfxClip()
    {
        var clip = Random.Range(0, HitSfxClips.Count);
        return HitSfxClips[clip];
    }
    #endregion

    #region Scene Loading
    public void LoadScene(int sceneNumber)
    {
        StartCoroutine(LoadSceneHelper(sceneNumber));
    }

    IEnumerator LoadSceneHelper(int sceneNumber)
    {
        AsyncOperation loading = SceneManager.LoadSceneAsync(sceneNumber);
        loading.allowSceneActivation = true;
        while (!loading.isDone)
        {
            yield return null;
        }
    }
    #endregion
}
