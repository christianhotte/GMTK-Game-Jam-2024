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
        else if (active && collision.collider.gameObject.TryGetComponent<CopBoid>(out CopBoid cb))
        {
            BlowUp();
        }
        else if (active && collision.collider.gameObject.TryGetComponent<CopBoidLeader>(out CopBoidLeader cbl))
        {
            BlowUp();
        }
        else if (collision.collider.gameObject.transform.parent != null)
        {
            if (active && collision.collider.gameObject.transform.parent.TryGetComponent<Asteroid>(out Asteroid asteroid))
            {
                asteroid.Damage(collisionDamage, velocity);
                BlowUp();
            }
        }
        
    }

    public void BlowUp()
    {
        if (TryGetComponent<PlayerController>(out PlayerController player)) { player.IsHit(); }
        else
        {
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
