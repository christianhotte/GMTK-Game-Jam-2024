using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuController : MonoBehaviour
{
    private PlayerControls playerControls;
    [SerializeField, Tooltip("The GameObject for the pause menu.")] private GameObject pauseMenu;
    [SerializeField, Tooltip("The GameObject for the game over menu.")] private GameObject gameOverMenu;
    [SerializeField, Tooltip("The game score text after a game over.")] private TextMeshProUGUI gameOverScoreText;
    [SerializeField, Tooltip("The high score text canvas group.")] private CanvasGroup highScoreCanvasGroup;
    [SerializeField, Tooltip("The flicker screen speed.")] private float flickerSpeed = 0.5f;

    private bool flickerHighScore = false;
    private float currentFlickerTimer;

    private void Awake()
    {
        playerControls = new PlayerControls();
        playerControls.ActionMap.Pause.started += _ => TogglePause();
        pauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        GameManager.Instance.isGameActive = true;
        GameManager.Instance.AudioManager.Play("GameMusic", 0.3f);
        highScoreCanvasGroup.alpha = 0f;
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
        Cursor.visible = true;
    }

    public void Resume()
    {
        GameManager.Instance.isPaused = false;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        GameManager.Instance?.AudioManager.ResumeAllSounds();
        Cursor.visible = false;
    }

    public void DisplayGameOver()
    {
        GameManager.Instance?.AudioManager.StopAllSounds();
        gameOverScoreText.text = "Score: " + ScoreManager.Instance.GetScore().ToString("n0");

        if(ScoreManager.Instance.GetScore() > PlayerPrefs.GetInt("Highscore"))
        {
            PlayerPrefs.SetInt("Highscore", Mathf.RoundToInt(ScoreManager.Instance.GetScore()));
            highScoreCanvasGroup.alpha = 1f;
            flickerHighScore = true;
        }

        Time.timeScale = 0f;
        gameOverMenu.SetActive(true);
        Cursor.visible = true;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        GameManager.Instance.isGameActive = true;
        GameManager.Instance.isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Cursor.visible = false;
    }

    public void ReturnToMain()
    {
        GameManager.Instance.isPaused = false;
        Time.timeScale = 1f;
        GameManager.Instance?.AudioManager.StopAllSounds();
        SceneManager.LoadScene(0);
    }

    public void ClickSoundUI()
    {
        GameManager.Instance.AudioManager.PlayOneShot("Click", PlayerPrefs.GetFloat("AudioVolume", 0.5f));
    }

    private void Update()
    {
        if (flickerHighScore)
        {
            currentFlickerTimer += Time.unscaledDeltaTime;

            if(currentFlickerTimer >= flickerSpeed)
            {
                highScoreCanvasGroup.alpha = highScoreCanvasGroup.alpha == 1 ? 0 : 1;
                currentFlickerTimer = 0;
            }
        }
    }
}
