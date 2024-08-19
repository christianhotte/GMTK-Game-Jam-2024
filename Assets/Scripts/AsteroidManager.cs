using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidManager : MonoBehaviour
{
    public static AsteroidManager main;
    [SerializeField, Tooltip("The Asteroid prefab.")] private Asteroid asteroidPrefab;
    [SerializeField, Tooltip("The Prison prefab.")] private GameObject prisonPrefab;
    [SerializeField, Tooltip("The Cop Boid Leader prefab.")] private CopBoidLeader copLeaderPrefab;
    [SerializeField, Tooltip("The Cop Boid prefab.")] private CopBoid copPrefab;
    [SerializeField, Tooltip("The asteroid container.")] private Transform asteroidContainer;
    [SerializeField, Tooltip("The maximum pool size for the asteroid.")] private int maxPoolSize;
    [SerializeField, Tooltip("The amount of distance to move to trigger an asteroid spawning.")] private float tresholdToSpawn;
    [SerializeField, Tooltip("The distance to spawn an asteroid at.")] private float spawnDistance;
    [SerializeField, Tooltip("The multiplier to control the scaling of the distance.")] private float distanceMultiplier;
    [SerializeField, Tooltip("The max angle to offset the asteroid spawn at.")] private float maxAngleDeviation;
    [SerializeField, Tooltip("The starting size for asteroids.")] private float startingSize = 4f;
    [SerializeField, Tooltip("The amount the size of the asteroids increase per ship.")] private float sizeIncrement = 1f;
    [SerializeField, Tooltip("The percent chance that a prison will spawn instead of an asteroid.")] private float prisonPercentage = 0.15f;
    [SerializeField, Tooltip("The percent chance that a cop swarm will spawn instead of an asteroid.")] private float copLeaderPercentage = 0.08f;

    public float BASE_SPEED = 10f;
    public float SPEED_FACTOR = 5f;
    public float HEALTH_FACTOR = 30f;
    public float ROTATION_MAX = 100f;
    public bool showGizmos = true;

    private PlayerController player;

    private List<Asteroid> asteroidPool = new List<Asteroid>();
    private List<PrisonController> prisonPool = new List<PrisonController>();
    private List<CopBoidLeader> copBoidLeaderPool = new List<CopBoidLeader>();
    private Camera mainCamera;

    private Vector2 lastPosition;
    private float currentAmountMoved;

    private void Awake()
    {
        main = this;
    }
    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        mainCamera = Camera.main;
        lastPosition = mainCamera.transform.position;
    }
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere((Vector2)Camera.main.transform.position, PlayerController.main.boidSettings.asteroidSpawnDistance + (PlayerController.main.boidSettings.asteroidSizeRange.x / 2));
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere((Vector2)Camera.main.transform.position, PlayerController.main.boidSettings.asteroidSpawnDistance + (PlayerController.main.boidSettings.asteroidSizeRange.y / 2));
        }
    }

    private void Update()
    {
        float distanceMoved = Vector2.Distance(PlayerController.main.transform.position, lastPosition);
        currentAmountMoved += distanceMoved;
        lastPosition = PlayerController.main.transform.position;

        if (currentAmountMoved >= PlayerController.main.boidSettings.asteroidSpawnRate)
        {
            //Spawn cops
            if (UnityEngine.Random.Range(0.0f, 1.0f) < copLeaderPercentage)
            {
                Vector2 spawnPosition = CreateSpawnPoint(10.0f);
                SpawnCopBoidLeader(spawnPosition);
            }

            //Spawn a prison
            else if(UnityEngine.Random.Range(0.0f, 1.0f) < prisonPercentage)
            {
                Vector2 spawnPosition = CreateSpawnPoint(prisonPrefab.GetComponentInChildren<BoxCollider2D>().size.x);

                SpawnPrison(spawnPosition);
            }

            //Spawn an asteroid
            else
            {
                float asteroidSize = CalculateAsteroidSize();
                Vector2 spawnPosition = CreateSpawnPoint(asteroidSize);

                if (asteroidPool.Count >= maxPoolSize)
                {
                    Asteroid moveAsteroid = FindLongestUnseenAsteroid();
                    moveAsteroid.transform.position = spawnPosition;
                    moveAsteroid.transform.localScale = asteroidSize * Vector3.one;
                    moveAsteroid.timeSinceLastSeen = -1;
                }
                else
                {
                    SpawnAsteroid(spawnPosition, asteroidSize);
                }
            }


            currentAmountMoved = 0;
        }
    }

    private Vector2 CreateSpawnPoint(float asteroidSize)
    {
        Vector2 spawnPosition = Vector2.zero;

        for (int i = 0; i < 100; i++)
        {
            spawnPosition = GenerateSpawnPosition(asteroidSize);

            if (!IsPositionOccupied(spawnPosition, asteroidSize * asteroidPrefab.GetRadius()))
                return spawnPosition;
        }

        return spawnPosition;
    }

    private Vector2 GenerateSpawnPosition(float asteroidSize)
    {
        Vector2 baseDirection = player.velocity.normalized;
        float randomAngle = UnityEngine.Random.Range(-maxAngleDeviation, maxAngleDeviation);
        Vector2 rotatedDirection = RotatePositionByAngle(baseDirection, randomAngle).normalized;
        return (Vector2)Camera.main.transform.position + (rotatedDirection * (PlayerController.main.boidSettings.asteroidSpawnDistance + (asteroidSize / 2)));
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
            if (asteroidPool[i].GetTimeSinceLastSeen() != -1 && asteroidPool[i].GetTimeSinceLastSeen() > longestUnseen.GetTimeSinceLastSeen())
                longestUnseen = asteroidPool[i];
        }

        return longestUnseen;
    }

    public PrisonController SpawnPrison()
    {
        Vector2 spawnPosition = CreateSpawnPoint(prisonPrefab.GetComponentInChildren<BoxCollider2D>().size.x);
        return SpawnPrison(spawnPosition);
    }

    public PrisonController SpawnPrison(Vector2 position)
    {
        PrisonController newPrison = Instantiate(prisonPrefab, position, Quaternion.identity, asteroidContainer).GetComponentInChildren<PrisonController>();
        newPrison.Init();
        prisonPool.Add(newPrison);
        return newPrison;
    }

    public CopBoidLeader SpawnCopBoidLeader(Vector2 position)
    {
        if (CopBoidLeader.instance != null) return null;
        CopBoidLeader temp = Instantiate(copLeaderPrefab, position, Quaternion.identity, asteroidContainer);
        for (int i = 0; i < PlayerController.main.ships.Count; i++)
        {
            float angle = (360f / PlayerController.main.ships.Count) * i;
            Vector2 smallpos = Quaternion.AngleAxis(20f, Vector3.forward) * Vector2.right*0.1f;
            smallpos += position;
            CopBoid cb = Instantiate(copPrefab, smallpos, Quaternion.identity, asteroidContainer);
            cb.leader = temp;
            cb.Init();
        }
        copBoidLeaderPool.Add(temp);
        return temp;
    }

    public Asteroid SpawnAsteroid(Vector2 position, float size, bool isChild = false)
    {
        Asteroid newAsteroid = Instantiate(asteroidPrefab, position, Quaternion.identity, asteroidContainer);
        if (!isChild)
        {
            newAsteroid.Init(size);
            asteroidPool.Add(newAsteroid);
        }

        return newAsteroid;
    }

    private float CalculateAsteroidSize()
    {
        //Uses a logarithmic function to increase the size of the asteroids and have it taper off as they get much larger
        return UnityEngine.Random.Range(PlayerController.main.boidSettings.asteroidSizeRange.x, PlayerController.main.boidSettings.asteroidSizeRange.y);
    }

    public void DestroyAsteroid(Asteroid currentAsteroid)
    {
        GameManager.Instance.AudioManager.PlayOneShot("AsteroidExplode", 0.5f);
        asteroidPool.Remove(currentAsteroid);
        Destroy(currentAsteroid.gameObject);
    }
}
