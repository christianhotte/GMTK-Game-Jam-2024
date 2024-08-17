using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class Asteroid : MonoBehaviour
{
    [SerializeField, Tooltip("The speed of the asteroid.")] private float speed;
    [SerializeField, Tooltip("The health of the asteroid.")] private float health;
    [SerializeField, Tooltip("The size of the asteroid.")] private float size;
    [SerializeField, Tooltip("The size of the asteroid.")] private float rotationSpeed;
    [SerializeField, Tooltip("The speed at which asteroids flicker.")] private float flickerSpeed;
    [SerializeField, Tooltip("The duration at which asteroids flicker.")] private float flickerDuration;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb2D;
    private Color defaultColor;
    private Vector2 velocity;

    private float currentHealth;
    private bool isFlickering;
    private float currentFlickerTime;
    private float currentFlickerDurationTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb2D = GetComponent<Rigidbody2D>();
        defaultColor = spriteRenderer.color;
        Init();
    }

    public void Init()
    {
        velocity = GetRandomDirection();
        transform.localScale = Vector3.one * size;
        currentHealth = health;
    }

    public void Init(float speed, float health, float size, float rotationSpeed)
    {
        this.speed = speed;
        this.health = health;
        this.size = size;
        this.rotationSpeed = rotationSpeed;
        Init();
    }

    private Vector2 GetRandomDirection() => new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));

    public void Damage(float damage, float explosionPower)
    {
        currentHealth -= damage;

        if(currentHealth <= 0)
        {
            currentHealth = 0;

            if(size / 2 >= 1f)
            {
                StartSpawn(Random.Range(2, 4), explosionPower, speed * 1.05f, (Random.Range(size / 4, size / 2)), rotationSpeed);
            }

            Destroy(gameObject);
        }

        currentFlickerTime = flickerSpeed;
        currentFlickerDurationTime = 0f;
        isFlickering = true;
    }

    protected void StartSpawn(int amount, float explosionPower, float speed, float size, float rotationSpeed)
    {
        Asteroid[] asteroids = new Asteroid[amount];

        for(int i = 0; i < amount; i++)
        {
            asteroids[i] = Instantiate(Resources.Load<Asteroid>("Asteroid"), transform.position, Quaternion.identity);
            asteroids[i].Init(speed, health / (this.size / size), size, rotationSpeed);
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
    }

    private void FixedUpdate()
    {
        rb2D.velocity = velocity * speed * Time.deltaTime;
        rb2D.angularVelocity = rotationSpeed;
    }

    private void FlickerAnimation()
    {
        currentFlickerDurationTime += Time.deltaTime;

        if(currentFlickerDurationTime >= flickerDuration)
        {
            isFlickering = false;
            spriteRenderer.color = defaultColor;
        }

        else
        {
            currentFlickerTime += Time.deltaTime;

            if(currentFlickerTime >= flickerSpeed)
            {
                spriteRenderer.color = spriteRenderer.color == Color.black ? Color.white : Color.black;
                currentFlickerTime = 0f;
            }
        }
    }

    public Vector2 GetVelocity() => velocity;

    public void SetVelocity(Vector2 velocity)
    {
        this.velocity = velocity;
    }
}
