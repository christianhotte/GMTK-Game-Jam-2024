using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidManager : MonoBehaviour
{
    public static AsteroidManager main;
    [SerializeField, Tooltip("The Asteroid prefab.")] private Asteroid asteroidPrefab;
    [SerializeField, Tooltip("The Prison prefab.")] private PrisonController prisonPrefab;
    [SerializeField, Tooltip("The Prison prefab.")] private PlanetController planetPrefab;
    [SerializeField, Tooltip("The Prison prefab.")] private GemController gemPrefab;
    [SerializeField, Tooltip("The object container.")] private Transform objectContainer;
    [SerializeField, Tooltip("The maximum pool size for the asteroid.")] private int maxPoolSize;
    [SerializeField, Tooltip("The amount of distance to move to trigger an asteroid spawning.")] private float thresholdToSpawn;
    [SerializeField, Tooltip("The distance to spawn an asteroid at.")] private float spawnDistance;
    [SerializeField, Tooltip("The multiplier to control the scaling of the distance.")] private float distanceMultiplier;
    [SerializeField, Tooltip("The max angle to offset the asteroid spawn at.")] private float maxAngleDeviation;
    [SerializeField, Tooltip("The starting size for asteroids.")] private float startingSize = 4f;
    [SerializeField, Tooltip("The amount the size of the asteroids increase per ship.")] private float sizeIncrement = 1f;
    [SerializeField, Tooltip("The percent chance that a prison will spawn instead of an asteroid.")] private float prisonPercentage = 0.15f;
    [SerializeField, Tooltip("The amount of ships needed before planets begin to spawn.")] private int planetThreshold = 50;
    [SerializeField, Tooltip("The percent chance that a planet will spawn instead of an asteroid.")] private float planetPercentage = 0.15f;

    public float BASE_SPEED = 10f;
    public float SPEED_FACTOR = 5f;
    public float HEALTH_FACTOR = 30f;
    public float ROTATION_MAX = 100f;
    public bool showGizmos = true;

    private PlayerController player;

    internal List<Asteroid> asteroidPool = new List<Asteroid>();
    private List<PrisonController> prisonPool = new List<PrisonController>();
    private List<PlanetController> planetPool = new List<PlanetController>();
    private List<GemController> gemPool = new List<GemController>();
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
        CreateObjectPools();
    }

    private void CreateObjectPools()
    {
        for (int i = 0; i < maxPoolSize; i++)
        {
            planetPool.Add(Instantiate(planetPrefab, Vector2.zero, Quaternion.identity, objectContainer));
            planetPool[i].gameObject.SetActive(false);

            prisonPool.Add(Instantiate(prisonPrefab, Vector2.zero, Quaternion.identity, objectContainer));
            prisonPool[i].gameObject.SetActive(false);

            asteroidPool.Add(Instantiate(asteroidPrefab, Vector2.zero, Quaternion.identity, objectContainer));
            asteroidPool[i].gameObject.SetActive(false);

            gemPool.Add(Instantiate(gemPrefab, Vector2.zero, Quaternion.identity, objectContainer));
            gemPool[i].gameObject.SetActive(false);
        }
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
            Vector2 spawnPosition;

            //Spawn a planet
            if (PlayerController.main.ships.Count >= planetThreshold && Random.Range(0.0f, 1.0f) < planetPercentage)
            {
                Debug.Log("Spawning A Planet...");
                spawnPosition = CreateSpawnPoint(planetPrefab.GetComponent<CircleCollider2D>().radius);

                SpawnPlanet(spawnPosition);
                currentAmountMoved = 0;
                return;
            }

            //Spawn a prison
            if (Random.Range(0.0f, 1.0f) < prisonPercentage)
            {
                Debug.Log("Spawning A Prison...");
                spawnPosition = CreateSpawnPoint(prisonPrefab.GetComponent<BoxCollider2D>().size.x);

                SpawnPrison(spawnPosition);
                currentAmountMoved = 0;
                return;
            }

            //Spawn an asteroid
            Debug.Log("Spawning An Asteroid...");
            float asteroidSize = CalculateAsteroidSize();
            spawnPosition = CreateSpawnPoint(asteroidSize);

            SpawnAsteroid(spawnPosition, asteroidSize);

            /*            if (asteroidPool.Count >= maxPoolSize)
                        {
                            Asteroid moveAsteroid = FindLongestUnseenAsteroid();
                            moveAsteroid.transform.position = spawnPosition;
                            moveAsteroid.transform.localScale = asteroidSize * Vector3.one;
                            moveAsteroid.timeSinceLastSeen = -1;
                        }
                        else
                        {
                            SpawnAsteroid(spawnPosition, asteroidSize);
                        }*/

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
        float randomAngle = Random.Range(-maxAngleDeviation, maxAngleDeviation);
        Vector2 rotatedDirection = RotatePositionByAngle(baseDirection, randomAngle).normalized;
        return (Vector2)GameManager.Instance.mainCamera.transform.position + (rotatedDirection * (PlayerController.main.boidSettings.asteroidSpawnDistance + (asteroidSize / 2)));
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
        asteroidPool.Remove(longestUnseen);
        asteroidPool.Add(longestUnseen);
        /*
        for (int i = 1; i < asteroidPool.Count; i++)
        {
            if (asteroidPool[i].GetTimeSinceLastSeen() != -1 && asteroidPool[i].GetTimeSinceLastSeen() > longestUnseen.GetTimeSinceLastSeen())
                longestUnseen = asteroidPool[i];
        }*/

        return longestUnseen;
    }

    public PlanetController SpawnPlanet(Vector2 position)
    {
        PlanetController newPlanet = null;
        for (int i = 0; i < planetPool.Count; i++)
        {
            if (!planetPool[i].gameObject.activeInHierarchy)
            {
                newPlanet = planetPool[i];
                newPlanet.gameObject.SetActive(true);
                newPlanet.transform.position = position;
                newPlanet.transform.SetParent(objectContainer);
                break;
            }
        }
        return newPlanet;
    }

    public PrisonController SpawnPrison()
    {
        Vector2 spawnPosition = CreateSpawnPoint(prisonPrefab.GetComponent<BoxCollider2D>().size.x);
        return SpawnPrison(spawnPosition);
    }

    public PrisonController SpawnPrison(Vector2 position)
    {
        PrisonController newPrison = null;
        for (int i = 0; i < prisonPool.Count; i++)
        {
            if (!prisonPool[i].gameObject.activeInHierarchy)
            {
                newPrison = prisonPool[i];
                newPrison.gameObject.SetActive(true);
                newPrison.transform.position = position;
                newPrison.transform.SetParent(objectContainer);
                newPrison.Init();
                break;
            }
        }
        return newPrison;
    }

    public Asteroid SpawnAsteroid(Vector2 position, float size, bool isChild = false)
    {
        Asteroid newAsteroid = null;
        for (int i = 0; i < asteroidPool.Count; i++)
        {
            if (!asteroidPool[i].gameObject.activeInHierarchy)
            {
                newAsteroid = asteroidPool[i];
                newAsteroid.gameObject.SetActive(true);
                newAsteroid.transform.position = position;
                newAsteroid.transform.SetParent(objectContainer);
                break;
            }
        }

        if (!isChild)
            newAsteroid.Init(size);

        return newAsteroid;
    }

    public GemController SpawnGem(Vector2 position)
    {
        GemController newGem = null;
        for (int i = 0; i < gemPool.Count; i++)
        {
            if (!planetPool[i].gameObject.activeInHierarchy)
            {
                newGem = gemPool[i];
                newGem.gameObject.SetActive(true);
                newGem.transform.position = position;
                newGem.transform.SetParent(objectContainer);
                break;
            }
        }

        return newGem;
    }

    private float CalculateAsteroidSize()
    {
        //Uses a logarithmic function to increase the size of the asteroids and have it taper off as they get much larger
        return Random.Range(PlayerController.main.boidSettings.asteroidSizeRange.x, PlayerController.main.boidSettings.asteroidSizeRange.y);
    }

    public void DestroyAsteroid(Asteroid currentAsteroid, float size)
    {
        int random = Random.Range(0, 3);

        float pitchFactor = 5f / size;
        Mathf.Clamp(pitchFactor, 0.1f, 2.9f);

        if (random == 0) GameManager.Instance.AudioManager.PlayOneShot("AsteroidExplode", PlayerPrefs.GetFloat("AudioVolume", 0.5f), (pitchFactor - 0.3f), (pitchFactor + 0.3f));
        else if (random == 1) GameManager.Instance.AudioManager.PlayOneShot("AsteroidExplode2", PlayerPrefs.GetFloat("AudioVolume", 0.5f), (pitchFactor - 0.3f), (pitchFactor + 0.3f));
        else GameManager.Instance.AudioManager.PlayOneShot("AsteroidExplode3", PlayerPrefs.GetFloat("AudioVolume", 0.5f), (pitchFactor - 0.3f), (pitchFactor + 0.3f));
        currentAsteroid.gameObject.SetActive(false);
    }
}
