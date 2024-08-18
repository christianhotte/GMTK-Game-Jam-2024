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
            asteroid.Damage(collisionDamage);
            if (TryGetComponent<PlayerController>(out PlayerController player)) { player.IsHit(); }
            else
            {
                PlayerController.main.ships.Remove(this);
                Destroy(gameObject);
            }
        }
    }
}
