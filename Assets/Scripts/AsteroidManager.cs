using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidManager : MonoBehaviour
{
    [SerializeField, Tooltip("The Asteroid prefab.")] private Asteroid asteroidPrefab;
    [SerializeField, Tooltip("The Prison prefab.")] private PrisonController prisonPrefab;
    [SerializeField, Tooltip("The asteroid container.")] private Transform asteroidContainer;
    [SerializeField, Tooltip("The maximum pool size for the asteroid.")] private int maxPoolSize;
    [SerializeField, Tooltip("The amount of distance to move to trigger an asteroid spawning.")] private float tresholdToSpawn;
    [SerializeField, Tooltip("The distance to spawn an asteroid at.")] private float spawnDistance;
    [SerializeField, Tooltip("The multiplier to control the scaling of the distance.")] private float distanceMultiplier;
    [SerializeField, Tooltip("The max angle to offset the asteroid spawn at.")] private float maxAngleDeviation;
    [SerializeField, Tooltip("The starting size for asteroids.")] private float startingSize = 4f;
    [SerializeField, Tooltip("The amount the size of the asteroids increase per ship.")] private float sizeIncrement = 1f;
    [SerializeField, Tooltip("The percent chance that a prison will spawn instead of an asteroid.")] private float prisonPercentage = 0.15f;

    public float BASE_SPEED = 10f;
    public float SPEED_FACTOR = 5f;
    public float HEALTH_FACTOR = 30f;
    public float ROTATION_MAX = 100f;

    private PlayerController player;

    private List<Asteroid> asteroidPool = new List<Asteroid>();
    private List<PrisonController> prisonPool = new List<PrisonController>();
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

        if (currentAmountMoved > (tresholdToSpawn + distanceMultiplier * Mathf.Log(CalculateAsteroidSize() + 1)))
        {

            //Spawn a prison
            if(Random.Range(0.0f, 1.0f) < prisonPercentage)
            {
                Vector2 spawnPosition = CreateSpawnPoint(prisonPrefab.GetComponent<BoxCollider2D>().size.x);

                SpawnPrison(spawnPosition);
            }

            //Spawn an asteroid
            else
            {
                Vector2 spawnPosition = CreateSpawnPoint(CalculateAsteroidSize() * asteroidPrefab.GetComponent<CircleCollider2D>().radius);

                if (asteroidPool.Count >= maxPoolSize)
                    FindLongestUnseenAsteroid().transform.position = spawnPosition;

                else
                    SpawnAsteroid(spawnPosition);
            }

            currentAmountMoved = 0;
        }
    }

    private Vector2 CreateSpawnPoint(float collisionRadius)
    {
        Vector2 spawnPosition = Vector2.zero;

        for (int i = 0; i < 100; i++)
        {
            spawnPosition = GenerateSpawnPosition();

            if (!IsPositionOccupied(spawnPosition, collisionRadius))
                return spawnPosition;
        }

        return spawnPosition;
    }

    private Vector2 GenerateSpawnPosition()
    {
        Vector2 baseDirection = player.velocity.normalized;
        float randomAngle = Random.Range(-maxAngleDeviation, maxAngleDeviation);
        Vector2 rotatedDirection = RotatePositionByAngle(baseDirection, randomAngle);
        return (Vector2)player.transform.position + rotatedDirection * (spawnDistance + distanceMultiplier * Mathf.Log(CalculateAsteroidSize() + 1));
    }

    private bool IsPositionOccupied(Vector2 spawnPosition, float collisionRadius)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(spawnPosition, collisionRadius);
        return hitColliders.Length > 0;
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

    public PrisonController SpawnPrison()
    {
        Vector2 spawnPosition = CreateSpawnPoint(prisonPrefab.GetComponent<BoxCollider2D>().size.x);
        return SpawnPrison(spawnPosition);
    }

    public PrisonController SpawnPrison(Vector2 position)
    {
        PrisonController newPrison = Instantiate(prisonPrefab, position, Quaternion.identity, asteroidContainer);
        newPrison.Init();
        prisonPool.Add(newPrison);
        return newPrison;
    }

    public Asteroid SpawnAsteroid(Vector2 position, bool isChild = false)
    {
        Asteroid newAsteroid = Instantiate(asteroidPrefab, position, Quaternion.identity, asteroidContainer);
        if (!isChild)
        {
            newAsteroid.Init(CalculateAsteroidSize());
            asteroidPool.Add(newAsteroid);
        }

        return newAsteroid;
    }

    private float CalculateAsteroidSize()
    {
        //Uses a logarithmic function to increase the size of the asteroids and have it taper off as they get much larger
        return startingSize + sizeIncrement * (Mathf.Log(PlayerController.main.ships.Count + 1) / Mathf.Log(10));
    }

    public void DestroyAsteroid(Asteroid currentAsteroid)
    {
        GameManager.Instance.AudioManager.PlayOneShot("AsteroidExplode", 0.5f);
        asteroidPool.Remove(currentAsteroid);
        Destroy(currentAsteroid.gameObject);
    }
}
