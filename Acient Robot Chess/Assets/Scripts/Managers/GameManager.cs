using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Helpers.Filesystem;
using Entities;
public class GameManager : Singleton<GameManager> {
    protected GameManager() { }

    //Setting Variables Stuff... (May change to a single class.)
    public float MusicVolume { get; private set; }
    public float SfxVolume { get; private set; }

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
