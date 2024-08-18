using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public AudioManager AudioManager { get; private set; }

    internal bool isPaused;
    internal bool isGameActive;

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
    }

    public void GameOver()
    {
        isGameActive = false;
        isPaused = true;
        FindObjectOfType<MenuController>()?.DisplayGameOver();
        Cursor.visible = true;
    }
}
