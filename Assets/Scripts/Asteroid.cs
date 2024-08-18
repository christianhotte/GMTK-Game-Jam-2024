using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Asteroid : MonoBehaviour
{
    private static readonly Vector2 ASTEROID_CHILDREN_RANGE = new Vector2(2, 5);
    private static readonly float ASTEROID_MINIMUM_SIZE = 0.5f;

    [SerializeField, Tooltip("The size of the asteroid.")] private float size;
    [SerializeField, Tooltip("The speed of the asteroid.")] private float speed;
    [SerializeField, Tooltip("The health of the asteroid.")] private float health;
    [SerializeField, Tooltip("The rotation speed of the asteroid.")] private float rotationSpeed;
    [SerializeField, Tooltip("The sprite renderer used for the main asteroid sprite.")] private SpriteRenderer spriteRenderer;
    [SerializeField, Tooltip("The sprite renderer used for the flash animation.")] private SpriteRenderer flashSpriteRenderer;
    [SerializeField, Tooltip("All possible sprites for the asteroids.")] private Sprite[] asteroidSprites;
    [SerializeField, Tooltip("All possible sprites for the asteroid overlays.")] private Sprite[] asteroidSpriteOverlays;
    [SerializeField, Tooltip("The speed at which asteroids flicker.")] private float flickerSpeed;
    [SerializeField, Tooltip("The duration at which asteroids flicker.")] private float flickerDuration;

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

    private void OnBecameVisible()
    {
        timeSinceLastSeen = 0f;
    }

    private Vector2 GetRandomDirection() => new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));

    public void Damage(float damage, float explosionPower)
    {
        currentHealth -= damage;

        if(currentHealth <= 0)
        {
            currentHealth = 0;

            if(size / ASTEROID_CHILDREN_RANGE.x >= ASTEROID_MINIMUM_SIZE)
            {
                int amount = Random.Range((int)ASTEROID_CHILDREN_RANGE.x, (int)ASTEROID_CHILDREN_RANGE.y);
                StartSpawn(amount, explosionPower, size / amount);
            }

            asteroidManager?.DestroyAsteroid(this);

        }

        currentFlickerTime = flickerSpeed;
        currentFlickerDurationTime = 0f;
        isFlickering = true;
    }

    protected void StartSpawn(int amount, float explosionPower, float size)
    {
        Asteroid[] asteroids = new Asteroid[amount];

        for(int i = 0; i < amount; i++)
        {
            asteroids[i] = asteroidManager?.SpawnAsteroid(transform.position, true);
            asteroids[i].Init(size);
            asteroids[i].SetVelocity((asteroids[i].GetVelocity() + velocity) * explosionPower);
        }
    }

    public bool debugDamage = false;

    private void Update()
    {
        if (isFlickering)
        {
            FlickerAnimation();
        }

        if (debugDamage)
        {
            debugDamage = false;
            Damage(1, 2);
        }

        timeSinceLastSeen += Time.deltaTime;
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

    public void SetVelocity(Vector2 velocity)
    {
        this.velocity = velocity;
    }

    public float GetTimeSinceLastSeen() => timeSinceLastSeen;
}
