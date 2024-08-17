using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private MenuPage currentMenuPage;

    private void Start()
    {
        currentMenuPage = menuStates[(int)MenuState.TITLE];
    }

    /// <summary>
    /// Transitions to the play state.
    /// </summary>
    public void PlayGame(string levelScene)
    {
        SceneManager.LoadScene(levelScene);
    }

    public void SwitchScene(int menuState)
    {
        switch ((MenuState)menuState)
        {
            //Go to the settings menu
            case MenuState.SETTINGS:
                currentMenuPage.menuCanvasGroup.alpha = 0;
                currentMenuPage.menuCanvasGroup.gameObject.SetActive(false);
                currentMenuPage = menuStates[(int)MenuState.SETTINGS];
                currentMenuPage.menuCanvasGroup.gameObject.SetActive(true);
                currentMenuPage.menuCanvasGroup.alpha = 1;
                break;

            //Go to the main menu
            default:
                currentMenuPage.menuCanvasGroup.alpha = 0;
                currentMenuPage.menuCanvasGroup.gameObject.SetActive(false);
                currentMenuPage = menuStates[(int)MenuState.TITLE];
                currentMenuPage.menuCanvasGroup.gameObject.SetActive(true
                    );
                currentMenuPage.menuCanvasGroup.alpha = 1;
                break;
        }
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
