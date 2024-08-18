using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    private PlayerControls playerControls;
    [SerializeField, Tooltip("The GameObject for the pause menu.")] private GameObject pauseMenu;
    [SerializeField, Tooltip("The GameObject for the game over menu.")] private GameObject gameOverMenu;

    private void Awake()
    {
        playerControls = new PlayerControls();
        playerControls.ActionMap.Pause.started += _ => TogglePause();
        pauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        GameManager.Instance.isGameActive = true;
    }

    private void OnEnable()
    {
        playerControls?.Enable();
    }

    private void OnDisable()
    {
        playerControls?.Disable();
    }

    private void TogglePause()
    {
        if (!GameManager.Instance.isGameActive)
            return;

        GameManager.Instance.isPaused = !GameManager.Instance.isPaused;

        if (GameManager.Instance.isPaused)
            Pause();
        else
            Resume();
    }

    public void Pause()
    {
        GameManager.Instance.isPaused = true;
        GameManager.Instance?.AudioManager.PauseAllSounds();
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        GameManager.Instance.isPaused = false;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        GameManager.Instance?.AudioManager.ResumeAllSounds();
    }

    public void DisplayGameOver()
    {
        GameManager.Instance?.AudioManager.StopAllSounds();
        Time.timeScale = 0f;
        gameOverMenu.SetActive(true);
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMain()
    {
        GameManager.Instance.isPaused = false;
        Time.timeScale = 1f;
        GameManager.Instance?.AudioManager.StopAllSounds();
        SceneManager.LoadScene(0);
    }
}
