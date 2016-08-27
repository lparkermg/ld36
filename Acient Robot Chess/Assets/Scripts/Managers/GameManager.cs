using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : Singleton<GameManager> {
    protected GameManager() { }

    //Setting Variables Stuff... (May change to a single class.)
    public float MusicVolume { get; private set; }
    public float SfxVolume { get; private set; }

    public bool IsDirty { get; private set; }

    public void Initialise()
    {
        //TODO: Load Settings etc here on initial load.
    }
    

    #region Setting IO etc.
    public void LoadSettings()
    {
        IsDirty = false;
    }

    public void SaveSettings()
    {
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
