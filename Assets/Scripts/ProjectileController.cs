using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    //Settings:
    public float baseVelocity;
    public float hitRadius;
    public float lifetime = 1;
    public float damage;
    public float doNotDamageSize = 1;

    //Runtime Variables:
    private float timeAlive;
    private Vector2 velocity;

    //UNITY METHODS:
    private void Update()
    {
        //Check for death by old age:
        timeAlive += Time.deltaTime;
        if (timeAlive >= lifetime)
        {
            Destroy(gameObject);
        }

        //Move and check for impact:
        Vector2 newPos = (Vector2)transform.position + (velocity * Time.deltaTime);
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, hitRadius, velocity, velocity.magnitude * Time.deltaTime);
        transform.position = newPos;
        if (hit.collider != null)
        {
            if(hit.collider.TryGetComponent(out Asteroid asteroid))
            {
                asteroid.Damage(damage, velocity.normalized);
                Destroy(gameObject);
                return;
            }

            if (hit.collider.TryGetComponent(out PrisonController prison))
            {
                prison.Damage(damage);
                Destroy(gameObject);
                return;
            }
        }
    }
    public void Fire(Transform barrel, Vector2 addVelocity)
    {
        transform.position = barrel.position;
        transform.rotation = barrel.rotation;
        velocity = ((Vector2)transform.up * baseVelocity) + addVelocity;
    }
}
