using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidShip : MonoBehaviour
{
    //Settings:
    public float collisionDamage = 50;
    public float maxAsteroidEatSize = 1;
    public float spawnForce = 1;

    //Runtime Vars:
    internal Vector2 velocity;
    internal bool active = true;

    //BOID METHODS:
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (active && collision.collider.TryGetComponent<Asteroid>(out Asteroid asteroid))
        {
            if (asteroid.GetSize() <= maxAsteroidEatSize) //Ship eats asteroid
            {
                asteroid.Damage(99999999, velocity);
                Transform newShip = Instantiate(PlayerController.main.boidPrefab.transform);
                ScoreManager.Instance.AddToScore(1);
                PlayerController.main.ships.Add(newShip.GetComponent<BoidShip>());
                newShip.position = transform.position;
                newShip.rotation = transform.rotation;
                newShip.GetComponent<BoidShip>().velocity = velocity + (-velocity.normalized * spawnForce);
                PlayerController.main.UpdateBoidSettings();
            }
            else //Asteroid destroys ship
            {
                asteroid.Damage(collisionDamage, velocity);
                if (TryGetComponent<PlayerController>(out PlayerController player)) { player.IsHit(); }
                else
                {
                    PlayerController.main.ships.Remove(this);
                    Destroy(gameObject);
                }
            }
        }
    }
}
