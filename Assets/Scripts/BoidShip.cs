using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidShip : MonoBehaviour
{
    //Objects
    public GameObject[] explosionParticles;

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

        if (active)
        {
            if (collision.collider.TryGetComponent<GemController>(out GemController gem))
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
            else
            {
                if (collision.transform.parent != null && collision.transform.parent.TryGetComponent<Asteroid>(out Asteroid asteroid))
                {
                    asteroid.Damage(collisionDamage, velocity);
                }
                if (TryGetComponent<PlayerController>(out PlayerController player)) { player.IsHit(); }
                else
                {
                    PlayerController.main.ships.Remove(this);
                    Destroy(gameObject);
                }

                //Particle Fx
                for (int i = 0; i < explosionParticles.Length; i++)
                {
                    var tempPart = Instantiate(explosionParticles[i], transform.position, explosionParticles[i].transform.rotation);
                }

                PlayerController.main.ships.Remove(this);
                Destroy(gameObject);
            }
        }
        
    }
}
