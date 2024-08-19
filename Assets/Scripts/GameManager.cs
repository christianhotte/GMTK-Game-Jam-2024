using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public AudioManager AudioManager { get; private set; }

    internal bool isPaused;
    internal bool isGameActive;

    internal float gemChance;

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

    public bool IsOnScreen(Vector3 position)
    {
        Vector2 viewportPos = Camera.main.WorldToViewportPoint(position);
        return (!(viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1));
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
