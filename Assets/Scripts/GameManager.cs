using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public AudioManager AudioManager { get; private set; }

    internal bool isPaused;
    internal bool isGameActive;

    internal float gemChance;
    internal Camera mainCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        AudioManager = GetComponentInChildren<AudioManager>();
        isPaused = false;
        isGameActive = false;

        gemChance = 100f;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        mainCamera = Camera.main;
    }

    public bool IsOnScreen(Transform item)
    {
        float distFromCam = Vector2.Distance(mainCamera.transform.position, item.position);
        return distFromCam <= PlayerController.main.boidSettings.asteroidSpawnDistance + item.transform.localScale.x / 2;
    }

    public bool CheckGem()
    {
        float randomRoll = Random.Range(1f, 100f);
        if (randomRoll <= gemChance)
        {
            gemChance = 15f;
            return true;
        }
        else
        {
            gemChance += 5f;
            Mathf.Clamp(gemChance, 0, 100);
            return false;
        }
    }

    public void GameOver()
    {
        isGameActive = false;
        isPaused = true;
        FindObjectOfType<MenuController>()?.DisplayGameOver();
        Cursor.visible = true;
    }
}
