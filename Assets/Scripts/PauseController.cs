using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    private PlayerControls playerControls;
    [SerializeField, Tooltip("The GameObject for the pause menu.")] private GameObject pauseMenu;

    private void Awake()
    {
        playerControls = new PlayerControls();
        playerControls.ActionMap.Pause.started += _ => TogglePause();
        pauseMenu.SetActive(false);
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

    public void ReturnToMain()
    {
        GameManager.Instance.isPaused = false;
        Time.timeScale = 1f;
        GameManager.Instance?.AudioManager.StopAllSounds();
        SceneManager.LoadScene(0);
    }
}
