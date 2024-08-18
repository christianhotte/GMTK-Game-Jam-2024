using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
public class Asteroid : MonoBehaviour
{
    private static readonly Vector2 ASTEROID_CHILDREN_RANGE = new Vector2(2, 5);
    private static readonly float ASTEROID_MINIMUM_SIZE = 0.5f;

    [SerializeField, Tooltip("The size of the asteroid.")] private float size;
    [SerializeField, Tooltip("The speed of the asteroid.")] private float speed;
    [SerializeField, Tooltip("The health of the asteroid.")] private float health;
    [SerializeField, Tooltip("The rotation speed of the asteroid.")] private float rotationSpeed;
    [SerializeField, Tooltip("The explosion force of the asteroid.")] private float explosionForce = 15f;
    [SerializeField, Tooltip("The sprite renderer used for the main asteroid sprite.")] private SpriteRenderer spriteRenderer;
    [SerializeField, Tooltip("The sprite renderer used for the flash animation.")] private SpriteRenderer flashSpriteRenderer;
    [SerializeField, Tooltip("All possible sprites for the asteroids.")] private Sprite[] asteroidSprites;
    [SerializeField, Tooltip("All possible sprites for the asteroid overlays.")] private Sprite[] asteroidSpriteOverlays;
    [SerializeField, Tooltip("The speed at which asteroids flicker.")] private float flickerSpeed;
    [SerializeField, Tooltip("The duration at which asteroids flicker.")] private float flickerDuration;
    [Space]
    [SerializeField, Tooltip("")] private float edibleSize = 1;
    [SerializeField, Tooltip("")] private float magnetRadius = 1;
    [SerializeField, Tooltip("")] private float magnetStrength = 1;

    private Rigidbody2D rb2D;
    private Vector2 velocity;

    private float timeSinceLastSeen;
    private float currentHealth;
    private bool isFlickering;
    private float currentFlickerTime;
    private float currentFlickerDurationTime;

    private AsteroidManager asteroidManager;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        flashSpriteRenderer.color = new Color(1, 1, 1, 0);
        asteroidManager = FindObjectOfType<AsteroidManager>();
        Init();
    }

    public void Init()
    {
        speed = asteroidManager.BASE_SPEED + asteroidManager.SPEED_FACTOR * Mathf.Log(size + 1);
        health = asteroidManager.HEALTH_FACTOR * Mathf.Sqrt(size);
        rotationSpeed = asteroidManager.ROTATION_MAX / Mathf.Sqrt(size + 1);

        int randomAsteroid = Random.Range(0, asteroidSprites.Length);
        spriteRenderer.sprite = asteroidSprites[randomAsteroid];
        flashSpriteRenderer.sprite = asteroidSpriteOverlays[randomAsteroid];
        velocity = GetRandomDirection();
        transform.localScale = Vector3.one * size;
        currentHealth = health;
        rb2D.AddForce(velocity * speed);
    }

    public void Init(float size)
    {
        this.size = size;
        Init();
    }

    private void OnOnscreen()
    {
        timeSinceLastSeen = 0f;
    }

    private Vector2 GetRandomDirection() => new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));

    public void Damage(float damage, Vector2 velocity)
    {
        currentHealth -= damage;

        if(currentHealth <= 0)
        {
            currentHealth = 0;

            if(size / ASTEROID_CHILDREN_RANGE.x >= ASTEROID_MINIMUM_SIZE)
            {
                int amount = Random.Range((int)ASTEROID_CHILDREN_RANGE.x, (int)ASTEROID_CHILDREN_RANGE.y);
                StartSpawn(amount, size / amount, velocity);
            }

            asteroidManager?.DestroyAsteroid(this);

        }

        currentFlickerTime = flickerSpeed;
        currentFlickerDurationTime = 0f;
        isFlickering = true;
    }

    protected void StartSpawn(int amount, float size, Vector2 velocity)
    {
        Asteroid[] asteroids = new Asteroid[amount];

        for(int i = 0; i < amount; i++)
        {
            asteroids[i] = asteroidManager?.SpawnAsteroid(transform.position, true);
            asteroids[i].Init(size);
            asteroids[i].AddExplosionForce(size * explosionForce, velocity);
        }
    }

    public bool debugDamage = false;

    private void Update()
    {
        Vector2 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        if (!(viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1))
            OnOnscreen();

            if (isFlickering)
        {
            FlickerAnimation();
        }

        if (debugDamage)
        {
            debugDamage = false;
            Damage(10, Vector2.zero);
        }

        timeSinceLastSeen += Time.deltaTime;

        if (size <= edibleSize)
        {
            BoidShip[] closeShips = Physics2D.OverlapCircleAll(transform.position, magnetRadius, LayerMask.GetMask("PlayerShip")).Select(o => o.GetComponent<BoidShip>()).ToArray();
            if (closeShips.Length > 0)
            {
                BoidShip closestShip = closeShips[0];
                float bestDist = Vector2.SqrMagnitude(transform.position - closestShip.transform.position);
                if (closeShips.Length > 1)
                {
                    for (int x = 1; x < closeShips.Length; x++)
                    {
                        float dist = Vector2.SqrMagnitude(transform.position - closeShips[x].transform.position);
                        if (dist < bestDist)
                        {
                            closestShip = closeShips[x];
                            bestDist = dist;
                        }
                    }
                }
                float interpolant = 1 - Mathf.InverseLerp(0, magnetRadius, bestDist);
                Vector2 magnetAccel = interpolant * magnetStrength * Time.deltaTime * (closestShip.transform.position - transform.position).normalized;
                transform.position = transform.position + (Vector3)magnetAccel;
            }
        }
    }

    private void FixedUpdate()
    {
        rb2D.angularVelocity = rotationSpeed;
    }

    private void FlickerAnimation()
    {
        currentFlickerDurationTime += Time.deltaTime;

        if(currentFlickerDurationTime >= flickerDuration)
        {
            isFlickering = false;
            flashSpriteRenderer.color = new Color(1, 1, 1, 0);
        }

        else
        {
            currentFlickerTime += Time.deltaTime;

            if(currentFlickerTime >= flickerSpeed)
            {
                flashSpriteRenderer.color = flashSpriteRenderer.color.a == 0 ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0);
                currentFlickerTime = 0f;
            }
        }
    }

    public Vector2 GetVelocity() => velocity;

    public void AddExplosionForce(float explosionForce, Vector2 velocity)
    {
        rb2D.AddForce(velocity * speed * explosionForce);
    }

    public void SetVelocity(Vector2 velocity)
    {
        this.velocity = velocity;
    }

    public float GetSize() => size;

    public float GetTimeSinceLastSeen() => timeSinceLastSeen;
}
