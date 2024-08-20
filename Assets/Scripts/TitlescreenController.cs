using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public enum MenuState { TITLE, SETTINGS };

[System.Serializable]
public class MenuPage
{
    public MenuState menuState;
    public CanvasGroup menuCanvasGroup;

    public MenuPage(MenuState menuState, CanvasGroup menuCanvasGroup)
    {
        this.menuState = menuState;
        this.menuCanvasGroup = menuCanvasGroup;
    }
}

public class TitlescreenController : MonoBehaviour
{
    [SerializeField, Tooltip("The states of the menu")] private MenuPage[] menuStates;

    [SerializeField, Tooltip("The text for the current high score.")] private TextMeshProUGUI highScoreText;
    [SerializeField] private Slider audioSlider;

    private MenuPage currentMenuPage;

    private void Start()
    {
        currentMenuPage = menuStates[(int)MenuState.TITLE];
        audioSlider.value = PlayerPrefs.GetFloat("AudioVolume", 0.5f) * 10f;
        highScoreText.text = "High Score: " + PlayerPrefs.GetInt("Highscore");
        GameManager.Instance.AudioManager.Play("TitleMusic", PlayerPrefs.GetFloat("AudioVolume", 0.5f));
    }

    /// <summary>
    /// Transitions to the play state.
    /// </summary>
    public void PlayGame(string levelScene)
    {
        GameManager.Instance.AudioManager.Stop("TitleMusic");
        SceneManager.LoadScene(levelScene);
    }

    public void SwitchScene(int menuState)
    {
        switch ((MenuState)menuState)
        {
            //Go to the settings menu
            case MenuState.SETTINGS:
                currentMenuPage.menuCanvasGroup.alpha = 0;
                currentMenuPage.menuCanvasGroup.interactable = false;
                currentMenuPage.menuCanvasGroup.blocksRaycasts = false;
                currentMenuPage.menuCanvasGroup.gameObject.SetActive(false);
                currentMenuPage = menuStates[(int)MenuState.SETTINGS];
                currentMenuPage.menuCanvasGroup.gameObject.SetActive(true);
                currentMenuPage.menuCanvasGroup.alpha = 1;
                currentMenuPage.menuCanvasGroup.interactable = true;
                currentMenuPage.menuCanvasGroup.blocksRaycasts = true;
                break;

            //Go to the main menu
            default:
                currentMenuPage.menuCanvasGroup.alpha = 0;
                currentMenuPage.menuCanvasGroup.interactable = false;
                currentMenuPage.menuCanvasGroup.blocksRaycasts = false;
                currentMenuPage.menuCanvasGroup.gameObject.SetActive(false);
                currentMenuPage = menuStates[(int)MenuState.TITLE];
                currentMenuPage.menuCanvasGroup.gameObject.SetActive(true
                    );
                currentMenuPage.menuCanvasGroup.alpha = 1;
                currentMenuPage.menuCanvasGroup.interactable = true;
                currentMenuPage.menuCanvasGroup.blocksRaycasts = true;
                break;
        }
    }

    public void AdjustVolume(float newVolume)
    {
        PlayerPrefs.SetFloat("AudioVolume", newVolume * 0.1f);
        GameManager.Instance.AudioManager.ChangeVolume("TitleMusic", PlayerPrefs.GetFloat("AudioVolume", 0.5f));
    }

    public void ClickSoundUI()
    {
        GameManager.Instance.AudioManager.PlayOneShot("Click", PlayerPrefs.GetFloat("AudioVolume", 0.5f));
    }

    /// <summary>
    /// Quits the game.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
