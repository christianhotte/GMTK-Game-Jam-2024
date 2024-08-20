using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetController : MonoBehaviour
{
    [SerializeField, Tooltip("The radius of the gravitational pull.")] private float gravitationalRadius = 20f;
    [SerializeField, Tooltip("The force of the gravitational pull.")] private float gravitationalForce = 50f;
    [SerializeField, Tooltip("The amount that the planet grows based on the size asteroid that collides with it.")] private float planetGrowthValue = 1f;
    [SerializeField, Tooltip("The max amount that an asteroid can make a planet grow.")] private float planetMaxGrowth = 0.1f;
    [SerializeField, Tooltip("The amount that the planet grows based on the size asteroid that collides with it.")] private float planetGrowthBuffer = 0.4f;

    private Rigidbody2D rb2D;
    private CircleCollider2D circleCollider;
    private float currentBufferTimer;
    private bool canGrow = true;
    private float originalGravitationalRadius;

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        originalGravitationalRadius = gravitationalRadius;
    }

    private void FixedUpdate()
    {
        // Find all objects within the gravitational field
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, gravitationalRadius);

        foreach (Collider2D spaceObject in objectsInRange)
        {
            if(spaceObject.TryGetComponent<BoidShip>(out BoidShip ship))
            {
                //Get the lerp value between the center and the edge of the radius
                float gravStrength = Mathf.InverseLerp(0, gravitationalRadius, Vector2.Distance(spaceObject.transform.position, transform.position));

                //Get the direction towards and velocity of the planet
                Vector2 directionToPlanet = (transform.position - ship.transform.position).normalized;
                Vector2 gravVel = gravStrength * gravitationalForce * directionToPlanet;

                //Get the value perpendicular (tangential) to the direction towards the planet
                Vector2 tangentialDir = new Vector2(-directionToPlanet.y, directionToPlanet.x);
                Vector2 tangentialVel = tangentialDir * ship.velocity.magnitude;

                //Combine the gravitational pull and the tangential velocity to provide a little bit of an orbit when moving
                if (ship.TryGetComponent<PlayerController>(out PlayerController player))
                    player.velocity += gravVel + tangentialVel;
                else
                    ship.velocity += gravVel + tangentialVel;
            }

            if(spaceObject.TryGetComponent<Asteroid>(out Asteroid asteroid))
            {
                //Get the lerp value between the center and the edge of the radius
                float gravStrength = Mathf.InverseLerp(0, gravitationalRadius, Vector2.Distance(spaceObject.transform.position, transform.position));

                //Get the direction towards and velocity of the planet
                Vector2 directionToPlanet = (transform.position - ship.transform.position).normalized;
                Vector2 gravVel = gravStrength * gravitationalForce * directionToPlanet;

                //Get the value perpendicular (tangential) to the direction towards the planet
                Vector2 tangentialDir = new Vector2(-directionToPlanet.y, directionToPlanet.x);
                Vector2 tangentialVel = tangentialDir * ship.velocity.magnitude;

                //Combine the gravitational pull and the tangential velocity to provide a little bit of an orbit when moving
                asteroid.pullVelocity += gravVel + tangentialVel;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.gameObject.transform.parent != null)
        {
            if (collision.collider.gameObject.transform.parent.TryGetComponent<Asteroid>(out Asteroid asteroid))
            {
                if (GameManager.Instance.IsOnScreen(asteroid.transform))
                {
                    asteroid.SpawnParticleExplosion();
                    if (canGrow)
                    {
                        transform.localScale = (Vector2)transform.localScale + (Vector2.one * Mathf.Clamp(Mathf.Log((planetGrowthValue * asteroid.GetSize()) + 1), 0, planetMaxGrowth));
                        gravitationalRadius = originalGravitationalRadius * transform.localScale.x;
                        currentBufferTimer = planetGrowthBuffer;
                        canGrow = false;
                    }
                    AsteroidManager.main.DestroyAsteroid(asteroid, asteroid.GetSize());
                }
                else
                    asteroid.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (!canGrow)
        {
            currentBufferTimer -= Time.deltaTime;
            if (currentBufferTimer <= 0f)
            {
                canGrow = true;
            }
        }
    }

    public CircleCollider2D GetCircleCollider() => circleCollider;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, gravitationalRadius);
    }
}
