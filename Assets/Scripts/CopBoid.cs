using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopBoid : MonoBehaviour
{
    //Settings:
    public int hp = 2;
    public float collisionDamage = 50;
    public float maxAsteroidEatSize = 1;
    public float spawnForce = 1;
    public float maxSpeedWhenLeaderDead = 5.0f;
    public float despawnDistance = 100.0f;
    // internal float currentBreakoffSpeed = 0.0f;

    public float flickerSpeed;
    public float flickerDuration;
    private float currentHealth;
    private bool isFlickering;
    private float currentFlickerTime;
    private float currentFlickerDurationTime;
    private SpriteRenderer flashSpriteRenderer;

    //Runtime Vars:
    internal Vector2 velocity;
    internal bool active = true;

    //Leader ref
    public CopBoidLeader leader;

    private void Awake()
    {
        flashSpriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        leader.ships.Add(this);
    }

    //BOID METHODS:
    private void OnCollisionEnter2D(Collision2D collision)
    {
        /*
        if (active && collision.collider.TryGetComponent<GemController>(out GemController gem))
        {
            Transform newShip = Instantiate(PlayerController.main.boidPrefab.transform);
            ScoreManager.Instance.AddToScore(1);
            PlayerController.main.ships.Add(newShip.GetComponent<BoidShip>());
            newShip.position = transform.position;
            newShip.rotation = transform.rotation;
            newShip.GetComponent<BoidShip>().velocity = velocity + (-velocity.normalized * spawnForce);
            PlayerController.main.UpdateBoidSettings();
            gem.DestroyGem();
        }
        else if (active && collision.collider.gameObject.transform.parent.TryGetComponent<Asteroid>(out Asteroid asteroid))
        {
            asteroid.Damage(collisionDamage, velocity);
            if (TryGetComponent<PlayerController>(out PlayerController player)) { player.IsHit(); }
            else
            {
                leader.ships.Remove(this);
                Destroy(gameObject);
            }
        }
        */
    }

    public void Damage()
    {
        hp -= 1;
        leader.alert = true;
        if (hp <= 1)
        {
            leader.ships.Remove(this);
            Destroy(gameObject);
        }
        currentFlickerTime = flickerSpeed;
        currentFlickerDurationTime = 0f;
        isFlickering = true;
    }

    public void Update()
    {
        if (isFlickering)
        {
            FlickerAnimation();
        }
        if (leader == null && active)
        {
            velocity = Vector3.MoveTowards(velocity, transform.up*maxSpeedWhenLeaderDead, Time.deltaTime);
            transform.position = transform.position + ((Vector3)velocity * Time.deltaTime);
            if (Vector3.Distance(PlayerController.main.transform.position, transform.position) > despawnDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    private void FlickerAnimation()
    {
        currentFlickerDurationTime += Time.deltaTime;

        if (currentFlickerDurationTime >= flickerDuration)
        {
            isFlickering = false;
            flashSpriteRenderer.color = new Color(1, 1, 1, 0);
        }

        else
        {
            currentFlickerTime += Time.deltaTime;

            if (currentFlickerTime >= flickerSpeed)
            {
                flashSpriteRenderer.color = flashSpriteRenderer.color.a == 0 ? new Color(0, 0, 0, 1) : new Color(0, 0, 0, 0);
                currentFlickerTime = 0f;
            }
        }
    }
}
