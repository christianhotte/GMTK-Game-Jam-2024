using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidManager : MonoBehaviour
{
    [SerializeField, Tooltip("The Asteroid prefab.")] private Asteroid asteroidPrefab;
    [SerializeField, Tooltip("The frequency for asteroids to spawn.")] private Vector2 frequency;
    [SerializeField, Tooltip("The distance range to spawn the asteroid away from the player.")] private Vector2 invalidRange;
    [SerializeField, Tooltip("The distance range to spawn the asteroid away from the player.")] private Vector2 spawnRange;

    private float currentFrequency;
    private float currentTimer;

    private PlayerController player;

    private List<Asteroid> asteroidPool = new List<Asteroid>();

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        currentFrequency = Random.Range(frequency.x, frequency.y);
    }

    private void Update()
    {
        currentTimer += Time.deltaTime;

        if(currentTimer >= currentFrequency)
        {
            Vector2 randomPos = player.transform.position;

            if (Random.Range(0, 2) == 0)
                randomPos.x += Random.Range(-spawnRange.x, -invalidRange.x);
            else
                randomPos.x += Random.Range(invalidRange.x, spawnRange.x);

            if (Random.Range(0, 2) == 0)
                randomPos.y += Random.Range(-spawnRange.y, -invalidRange.y);
            else
                randomPos.y += Random.Range(invalidRange.y, spawnRange.y);

            asteroidPool.Add(Instantiate(asteroidPrefab, randomPos, Quaternion.identity));
            currentTimer = 0;
            currentFrequency = Random.Range(frequency.x, frequency.y);
        }
    }

    public void DestroyAsteroid(Asteroid currentAsteroid)
    {
        asteroidPool.Remove(currentAsteroid);
        Destroy(currentAsteroid.gameObject);
    }
}
