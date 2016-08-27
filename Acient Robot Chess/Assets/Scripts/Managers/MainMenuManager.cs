using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuManager : MonoBehaviour {

    public CanvasGroup MainMenuGroup;
    public CanvasGroup OptionsMenuGroup;

    public Button SaveButton;
	// Use this for initialization
	void Start () {
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
        HideOptionsMenu();
        ShowMainMenu();
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

    private void HideOptionsMenu()
    {
        OptionsMenuGroup.alpha = 0.0f;
        OptionsMenuGroup.blocksRaycasts = false;
        OptionsMenuGroup.interactable = false;
    }

    private void ShowOptionsMenu()
    {
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
        MainMenuGroup.alpha = 1.0f;
        MainMenuGroup.blocksRaycasts = true;
        MainMenuGroup.interactable = true;
    }

    private void CheckIsDirty()
    {
        //TODO: Make IsDirty in GameManager for showing any changes are made to the settings and need saving.
        SaveButton.interactable = false;
    }
}
