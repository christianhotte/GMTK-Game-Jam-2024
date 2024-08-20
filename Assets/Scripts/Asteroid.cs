using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
public class Asteroid : MonoBehaviour
{
    private static readonly Vector2 ASTEROID_CHILDREN_RANGE = new Vector2(2, 5);

    [SerializeField, Tooltip("The size of the asteroid.")] private float size;
    public float minSpawnerSize = 4;
    [SerializeField, Tooltip("The speed of the asteroid.")] private float speed;
    [SerializeField, Tooltip("The health of the asteroid.")] private float health;
    [SerializeField, Tooltip("The radius of the asteroid.")] private float radius;
    [SerializeField, Tooltip("The rotation speed of the asteroid.")] private float rotationSpeed;
    [SerializeField, Tooltip("The explosion force of the asteroid.")] private float explosionForce = 15f;
    public float explosionRadius;
    [SerializeField, Tooltip("The sprite renderer used for the main asteroid sprite.")] private SpriteRenderer spriteRenderer;
    [SerializeField, Tooltip("The sprite renderer used for the flash animation.")] private SpriteRenderer flashSpriteRenderer;
    [SerializeField, Tooltip("All possible sprites for the asteroids.")] private Sprite[] asteroidSprites;
    [SerializeField, Tooltip("All possible sprites for the asteroid overlays.")] private Sprite[] asteroidSpriteOverlays;
    [SerializeField, Tooltip("The Explosion Sprite particle for this object")] private GameObject explosionParticle;
    [SerializeField, Tooltip("The speed at which asteroids flicker.")] private float flickerSpeed;
    [SerializeField, Tooltip("The duration at which asteroids flicker.")] private float flickerDuration;

    private Rigidbody2D rb2D;
    private Vector2 velocity;

    internal float timeSinceLastSeen = -1;
    private float currentHealth;
    private bool isFlickering;
    private float currentFlickerTime;
    private float currentFlickerDurationTime;

    private bool prevOnScreen = false;
    private AsteroidManager asteroidManager;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        flashSpriteRenderer.color = new Color(1, 1, 1, 0);
    }

    public void Init()
    {
        speed = AsteroidManager.main.BASE_SPEED + AsteroidManager.main.SPEED_FACTOR * Mathf.Log(size + 1);
        health = AsteroidManager.main.HEALTH_FACTOR * Mathf.Sqrt(size);
        rotationSpeed = AsteroidManager.main.ROTATION_MAX / Mathf.Sqrt(size + 1);

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
        Destroy(spriteRenderer.gameObject.GetComponent<PolygonCollider2D>());
        spriteRenderer.gameObject.AddComponent<PolygonCollider2D>();
    }

    private void OnEnable()
    {
        Init();
    }

    private void OnOnscreen()
    {
        AsteroidManager.main.asteroidPool.Remove(this);
        AsteroidManager.main.asteroidPool.Add(this);
        timeSinceLastSeen = 0f;
    }

    private Vector2 GetRandomDirection() => new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));

    private void OnDrawGizmos()
    {
        if (Application.isEditor)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, (size / 2) * explosionRadius);
        }
    }

    public void Damage(float damage, Vector2 velocity)
    {
        currentHealth -= damage;

        if(currentHealth <= 0)
        {
            currentHealth = 0;

            if (size >= minSpawnerSize)
            {
                int amount = Random.Range((int)ASTEROID_CHILDREN_RANGE.x, (int)ASTEROID_CHILDREN_RANGE.y);

                Asteroid[] asteroids = new Asteroid[amount];
                for (int i = 0; i < amount; i++)
                {
                    Vector2 spawnPoint = (Vector2)transform.position + ((size / 2) * explosionRadius * Random.insideUnitCircle.normalized);
                    float spawnSize = size / 2.5f;
                    asteroids[i] = AsteroidManager.main?.SpawnAsteroid(spawnPoint, spawnSize);
                    asteroids[i].AddExplosionForce(size * explosionForce, (spawnPoint - (Vector2)transform.position).normalized);
                }
            }
            else
            {
                if (GameManager.Instance.CheckGem())
                {
                    AsteroidManager.main?.SpawnGem(transform.position);
                }
            }

            //Spawn particle
            var particle = Instantiate(explosionParticle, transform.position, Quaternion.identity);
            ParticleSystem _particle = particle.GetComponent<ParticleSystem>();

            var main = _particle.main;

            //StartLifeTime
            float minSize = 0.1f * size;
            Mathf.Clamp(minSize, 0f, 1.5f);
            float maxSize = 0.3f * size;
            Mathf.Clamp(maxSize, 0f, 3.5f);
            main.startLifetime = new ParticleSystem.MinMaxCurve((minSize), (maxSize));

            //StartSpeed
            var sSpeed = main.startSpeed;
            sSpeed.constantMin = (2 * size);
            sSpeed.constantMax = (5 * size);

            //Burst Size
            int var = Mathf.RoundToInt(5 * size);
            Mathf.Clamp(var, short.MinValue, short.MaxValue);
            short min;
            min = (short)var;

            int _var = Mathf.RoundToInt(15 * size);
            Mathf.Clamp(_var, short.MinValue, short.MaxValue);
            short max;
            max = (short)_var;

            ParticleSystem.Burst _burst = new ParticleSystem.Burst(0, min, max, 1, 0);
            _particle.emission.SetBurst(0, _burst);

            //Shape
            var shape = _particle.shape;
            shape.radius = (0.14f * size);
            

            ScoreManager.Instance.AddToScore(Mathf.RoundToInt((size * 10.0f) / 10.0f) * 10);
            AsteroidManager.main?.DestroyAsteroid(this);

        }

        currentFlickerTime = flickerSpeed;
        currentFlickerDurationTime = 0f;
        isFlickering = true;
    }

    public bool debugDamage = false;

    private void Update()
    {

        bool onScreen = GameManager.Instance.IsOnScreen(transform);
        if (onScreen && !prevOnScreen) OnOnscreen();
        prevOnScreen = onScreen;

        if (isFlickering)
        {
            FlickerAnimation();
        }

        if (debugDamage)
        {
            debugDamage = false;
            Damage(10, Vector2.zero);
        }

        if (timeSinceLastSeen != -1) timeSinceLastSeen += Time.deltaTime;
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
                flashSpriteRenderer.color = flashSpriteRenderer.color.a == 0 ? new Color(0, 0, 0, 1) : new Color(0, 0, 0, 0);
                currentFlickerTime = 0f;
            }
        }
    }

    public Vector2 GetVelocity() => velocity;
    public float GetRadius() => radius;
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
