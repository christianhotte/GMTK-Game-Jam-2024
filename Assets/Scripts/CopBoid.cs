using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopBoid : MonoBehaviour
{
    public GameObject[] explosionParticles;

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
    public SpriteRenderer flashSpriteRenderer;

    //Runtime Vars:
    internal Vector2 velocity;
    internal bool active = true;
    private Rigidbody2D r2d;

    //Leader ref
    public CopBoidLeader leader;

    private void Awake()
    {
        r2d = GetComponent<Rigidbody2D>();
    }

    public void Init()
    {
        if (leader != null) leader.ships.Add(this);
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
        if (collision.collider.gameObject.transform.parent == null) return;
        if (active && collision.collider.gameObject.transform.parent.TryGetComponent<Asteroid>(out Asteroid asteroid))
        {
            Damage(false, 999);
            asteroid.Damage(collisionDamage, velocity);
        }
    }

    public void Damage(bool from_shooting, int amt)
    {
        hp -= amt;
        if (from_shooting && leader != null) leader.alert = true;
        GameManager.Instance.AudioManager.PlayOneShot("CopHit", PlayerPrefs.GetFloat("AudioVolume", 0.5f));
        if (hp <= 0)
        {
            //Particle Fx
            for (int i = 0; i < explosionParticles.Length; i++)
            {
                var tempPart = Instantiate(explosionParticles[i], transform.position, explosionParticles[i].transform.rotation);
            }
            leader.ships.Remove(this);
            GameManager.Instance.AudioManager.PlayOneShot("CopDeath", PlayerPrefs.GetFloat("AudioVolume", 0.5f));
            gameObject.SetActive(false);
        }
        flashSpriteRenderer.color = new Color(1, 1, 1, 0);
    }
    public void Damage(bool from_shooting)
    {
        Damage(from_shooting, 1);
    }

    public void Update()
    {
        r2d.velocity = Vector2.zero;
        r2d.angularVelocity = 0;
        FlickerAnimation();
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
                GameManager.Instance.AudioManager.PlayOneShot("CopDeath", PlayerPrefs.GetFloat("AudioVolume", 0.5f));
                gameObject.SetActive(false);
            }
        }
    }

    private void FlickerAnimation()
    {
        float temp = flashSpriteRenderer.color.a;
        temp = Mathf.Lerp(temp, 1, Time.deltaTime * 4.0f);
        flashSpriteRenderer.color = new Color(1, 1, 1, temp);
    }
}
