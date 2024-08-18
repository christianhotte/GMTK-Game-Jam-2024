using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidShip : MonoBehaviour
{
    //Settings:
    public float collisionDamage = 50;

    //Runtime Vars:
    internal Vector2 velocity;

    //BOID METHODS:
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<Asteroid>(out Asteroid asteroid))
        {
            //asteroid.Damage(50, )
        }
    }
}
