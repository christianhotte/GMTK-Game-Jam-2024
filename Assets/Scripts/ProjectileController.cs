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

    //Runtime Variables:
    private float timeAlive;
    private Vector2 velocity;

    //UNITY METHODS:
    private void Start()
    {
        velocity = (Vector2)transform.up * baseVelocity;
    }
    private void Update()
    {
        //Check for death by old age:
        timeAlive += Time.deltaTime;
        if (timeAlive >= lifetime)
        {
            Destroy(gameObject);
        }

        //Move and check for impact:
        Vector2 newPos = (Vector2)transform.position + velocity;

        transform.position = newPos;
    }
}
