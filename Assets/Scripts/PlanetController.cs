using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetController : MonoBehaviour
{
    [SerializeField, Tooltip("The radius of the gravitational pull.")] private float gravitationalRadius = 20f;
    [SerializeField, Tooltip("The force of the gravitational pull.")] private float gravitationalForce = 50f;

    private Rigidbody2D rb2D;

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
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
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, gravitationalRadius);
    }
}
