using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuManager : MonoBehaviour {

    public CanvasGroup MainMenuGroup;
    public CanvasGroup OptionsMenuGroup;

    public Slider MusicVolumeSlider;
    public Slider SfxVolumeSlider;

    public Button SaveButton;

    private bool _isDirty;
	// Use this for initialization
	void Start () {
        GameManager.Instance.LoadSettings();
        ShowMainMenu();
        
	}
	
	// Update is called once per frame
	void Update () {
        CheckIsDirty();
	}

    public void OptionsButtonClicked()
    {
        HideMainMenu();
        ShowOptionsMenu();
    }

    public void BackButtonClicked()
    {
        if (_isDirty)
            GameManager.Instance.LoadSettings();
        HideOptionsMenu();
        ShowMainMenu();
        
    }

    public void SaveButtonClicked()
    {
        GameManager.Instance.SaveSettings();
    }

    public void StartButtonClicked()
    {
        GameManager.Instance.StartGame();
    }

    public void DisplayOptionsMenu()
    {
        HideMainMenu();
        ShowOptionsMenu();
    }

    public void DisplayMainMenu()
    {
        HideOptionsMenu();
        ShowMainMenu();
    }

    public void MusicSliderUpdate()
    {
        GameManager.Instance.UpdateMusicVolume(MusicVolumeSlider.value);
    }

    public void SfxSliderUpdate()
    {
        GameManager.Instance.UpdateSfxVolume(SfxVolumeSlider.value);
    }

    #region Private Helper Functions
    private void HideOptionsMenu()
    {
        OptionsMenuGroup.alpha = 0.0f;
        OptionsMenuGroup.blocksRaycasts = false;
        OptionsMenuGroup.interactable = false;
    }

    private void ShowOptionsMenu()
    {
        MusicVolumeSlider.value = GameManager.Instance.MusicVolume;
        SfxVolumeSlider.value = GameManager.Instance.SfxVolume;

        OptionsMenuGroup.alpha = 1.0f;
        OptionsMenuGroup.blocksRaycasts = true;
        OptionsMenuGroup.interactable = true;
        //TODO: Setup display stuff here for when the GameManager is implemented.
    }

    private void HideMainMenu()
    {
        MainMenuGroup.alpha = 0.0f;
        MainMenuGroup.blocksRaycasts = false;
        MainMenuGroup.interactable = false;
    }

    private void ShowMainMenu()
    {
        if (_isDirty)
            GameManager.Instance.LoadSettings();
        MainMenuGroup.alpha = 1.0f;
        MainMenuGroup.blocksRaycasts = true;
        MainMenuGroup.interactable = true;
    }

    private void CheckIsDirty()
    {
        _isDirty = GameManager.Instance.IsDirty;
        SaveButton.interactable = _isDirty;
    }
    #endregion
}
