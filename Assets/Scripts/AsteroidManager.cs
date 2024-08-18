using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidManager : MonoBehaviour
{
    [SerializeField, Tooltip("The Asteroid prefab.")] private Asteroid asteroidPrefab;
    [SerializeField, Tooltip("The asteroid container.")] private Transform asteroidContainer;
    [SerializeField, Tooltip("The maximum pool size for the asteroid.")] private int maxPoolSize;
    [SerializeField, Tooltip("The amount of distance to move to trigger an asteroid spawning.")] private float tresholdToSpawn;
    [SerializeField, Tooltip("The distance to spawn an asteroid at.")] private float spawnDistance;
    [SerializeField, Tooltip("The max angle to offset the asteroid spawn at.")] private float maxAngleDeviation;
    [SerializeField, Tooltip("The starting size for asteroids.")] private float startingSize = 4f;
    [SerializeField, Tooltip("The amount the size of the asteroids increase per ship.")] private float sizeIncrement = 1f;

    public float BASE_SPEED = 10f;
    public float SPEED_FACTOR = 5f;
    public float HEALTH_FACTOR = 30f;
    public float ROTATION_MAX = 100f;

    private PlayerController player;

    private List<Asteroid> asteroidPool = new List<Asteroid>();
    private Camera mainCamera;

    private Vector2 lastPosition;
    private float currentAmountMoved;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        mainCamera = Camera.main;
        lastPosition = mainCamera.transform.position;
    }

    private void Update()
    {
        float distanceMoved = Vector2.Distance(mainCamera.transform.position, lastPosition);
        currentAmountMoved += distanceMoved;
        lastPosition = mainCamera.transform.position;

        if (currentAmountMoved > tresholdToSpawn)
        {
            Vector2 baseDirection = player.velocity.normalized;
            float randomAngle = Random.Range(-maxAngleDeviation, maxAngleDeviation);
            Vector2 rotatedDirection = RotatePositionByAngle(baseDirection, randomAngle);
            Vector2 spawnPosition = (Vector2)player.transform.position + rotatedDirection * spawnDistance;

            if (asteroidPool.Count >= maxPoolSize)
                FindLongestUnseenAsteroid().transform.position = spawnPosition;

            else
                SpawnAsteroid(spawnPosition);

            currentAmountMoved = 0;
        }
    }

    private Vector2 RotatePositionByAngle(Vector2 vector, float angle)
    {
        // Convert angle from degree to radians
        float rad = angle * Mathf.Deg2Rad;

        //Rotate the angle counter clockwise
        float x = vector.x * Mathf.Cos(rad) - vector.y * Mathf.Sin(rad);
        float y = vector.x * Mathf.Sin(rad) + vector.y * Mathf.Cos(rad);

        return new Vector2(x, y);
    }

    /// <summary>
    /// Returns the asteroid within the pool that has not been seen the longest.
    /// </summary>
    /// <returns></returns>
    public Asteroid FindLongestUnseenAsteroid()
    {
        if (asteroidPool.Count == 0)
            return null;

        Asteroid longestUnseen = asteroidPool[0];

        for (int i = 1; i < asteroidPool.Count; i++)
        {
            if (asteroidPool[i].GetTimeSinceLastSeen() > longestUnseen.GetTimeSinceLastSeen())
                longestUnseen = asteroidPool[i];
        }

        return longestUnseen;
    }

    public Asteroid SpawnAsteroid(Vector2 position, bool isChild = false)
    {
        Asteroid newAsteroid = Instantiate(asteroidPrefab, position, Quaternion.identity, asteroidContainer);
        if (!isChild)
        {
            newAsteroid.Init(startingSize + Mathf.Log(1 + PlayerController.main.ships.Count) * sizeIncrement);
            asteroidPool.Add(newAsteroid);
        }

        return newAsteroid;
    }

    public void DestroyAsteroid(Asteroid currentAsteroid)
    {
        GameManager.Instance.AudioManager.PlayOneShot("AsteroidExplode", 0.5f);
        asteroidPool.Remove(currentAsteroid);
        Destroy(currentAsteroid.gameObject);
    }
}
