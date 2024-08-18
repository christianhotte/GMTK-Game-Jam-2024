using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build.Content;
using System.Linq;
using UnityEngine;
using System;
using System.Security.Cryptography;
using Unity.PlasticSCM.Editor.WebApi;

public class CopBoidLeader : MonoBehaviour
{
    public int hp = 10;

    internal bool alert = false;
    public float alertDistance = 5.0f;

    internal float current_speed;
    public float max_speed = 5.0f;
    public float acceleration = 0.5f;

    public float flickerSpeed;
    public float flickerDuration;
    private float currentHealth;
    private bool isFlickering;
    private float currentFlickerTime;
    private float currentFlickerDurationTime;
    private SpriteRenderer flashSpriteRenderer;

    internal List<CopBoid> ships = new List<CopBoid>();
    public BoidSettings boidSettings;

    private void Awake()
    {
        flashSpriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (isFlickering)
        {
            FlickerAnimation();
        }
        //Move boids:
        foreach (CopBoid boid in ships)
        {
            // if (thrusting) boid.velocity += Time.deltaTime * thrustPower * boidSettings.boidLeaderVelocityInheritance * (Vector2)transform.up;
            boid.velocity -= Time.deltaTime * boidSettings.boidDragCoefficient * boid.velocity; //Apply drag
            boid.transform.position = boid.transform.position + ((Vector3)boid.velocity * Time.deltaTime);
            float boidTargetRot;
            if (alert)
                boidTargetRot = Vector3.SignedAngle(Vector3.up, PlayerController.main.transform.position - boid.transform.position, Vector3.forward);
            else
                boidTargetRot = Vector3.SignedAngle(Vector3.up, boid.transform.position - transform.position, Vector3.forward);
            boidTargetRot = Mathf.LerpAngle(boid.transform.eulerAngles.z, boidTargetRot, boidSettings.boidRotRate * Time.deltaTime);
            boid.transform.eulerAngles = Vector3.forward * boidTargetRot;
        }

        //rotate and move towards player
        if (alert)
        {
            current_speed = Mathf.MoveTowards(current_speed, max_speed, Time.deltaTime*acceleration);
            transform.position += transform.up * current_speed * Time.deltaTime;
            float targetRot = Vector3.SignedAngle(Vector3.up, PlayerController.main.transform.position - transform.position, Vector3.forward);
            targetRot = Mathf.LerpAngle(transform.eulerAngles.z, targetRot, boidSettings.boidRotRate * Time.deltaTime);
            transform.eulerAngles = Vector3.forward * targetRot;
        }
        else if (Vector3.Distance(PlayerController.main.transform.position, transform.position) < alertDistance)
        {
            alert = true;
        }
    }
    private void FixedUpdate()
    {
        //Flock ships:
        List<Vector2> newVelocities = new List<Vector2>();
        foreach (CopBoid boid in ships)
        {
            List<CopBoid> neighbors = Physics2D.OverlapCircleAll(boid.transform.position, boidSettings.boidNeighborRadius).Where(b => b.TryGetComponent(out CopBoid shipCont) && shipCont != boid).Select(b => b.GetComponent<CopBoid>()).ToList();
            Vector2 newBoidVel = boid.velocity;

            //Neighbor velocity checks:
            if (neighbors.Count > 0)
            {
                //Apply neighbor cohesion:
                Vector2 cohesionVelocity = Vector2.zero;
                Vector2 totalPosition = Vector2.zero;
                foreach (CopBoid neighbor in neighbors)
                {
                    totalPosition += (Vector2)neighbor.transform.position;
                }
                totalPosition /= neighbors.Count;
                Vector2 cohesionSep = (Vector2)boid.transform.position - totalPosition;
                cohesionVelocity = -boidSettings.boidCohForce * boidSettings.maxBoidForce * cohesionSep;
                cohesionVelocity = LimitMagnitude(cohesionVelocity, boidSettings.maxBoidSpeed);
                newBoidVel += cohesionVelocity;

                //Apply neighbor separation:
                Vector2 separationVelocity = Vector2.zero;
                int separatorNeighbors = 0;
                foreach (CopBoid neighbor in neighbors)
                {
                    Vector2 separation = boid.transform.position - neighbor.transform.position;
                    float sepDist = separation.magnitude;
                    if (sepDist < boidSettings.boidSeparationRadius)
                    {
                        separatorNeighbors++;
                        float sepStrength = Mathf.Clamp01(Mathf.InverseLerp(0, boidSettings.boidSeparationRadius, sepDist));
                        separationVelocity += boidSettings.maxBoidForce * boidSettings.boidSepForce * sepStrength * separation.normalized;
                    }
                }
                //if (separatorNeighbors > 0) separationVelocity /= separatorNeighbors;
                separationVelocity = LimitMagnitude(separationVelocity, boidSettings.maxBoidSpeed);
                newBoidVel += separationVelocity;
            }

            //Leader velocity check:
            Vector2 leaderSeparation = transform.position - boid.transform.position;
            float leaderSepDist = leaderSeparation.magnitude;
            if (leaderSepDist > boidSettings.boidLeaderSepRadii.x)
            {
                float sepStrength = Mathf.Clamp01(Mathf.InverseLerp(boidSettings.boidLeaderSepRadii.x, boidSettings.boidLeaderSepRadii.y, leaderSepDist));
                Vector2 followVelocity = boidSettings.maxBoidForce * boidSettings.boidLeaderFollowForce * sepStrength * leaderSeparation.normalized;
                followVelocity = LimitMagnitude(followVelocity, boidSettings.maxBoidSpeed);
                newBoidVel += followVelocity;
            }

            //Cleanup:
            newVelocities.Add(newBoidVel);
        }
        for (int x = 0; x < ships.Count; x++)
        {
            CopBoid boid = ships[x];
            boid.velocity = LimitMagnitude(newVelocities[x], boidSettings.maxBoidSpeed);
        }
    }
    private Vector2 LimitMagnitude(Vector2 baseVector, float maxMagnitude)
    {
        if (baseVector.sqrMagnitude > maxMagnitude * maxMagnitude)
        {
            baseVector = baseVector.normalized * maxMagnitude;
        }
        return baseVector;
    }

    public void Damage()
    {
        hp -= 1;
        alert = true;
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
        currentFlickerTime = flickerSpeed;
        currentFlickerDurationTime = 0f;
        isFlickering = true;
    }

    private void FlickerAnimation()
    {
        currentFlickerDurationTime += Time.deltaTime;

        if (currentFlickerDurationTime >= flickerDuration)
        {
            isFlickering = false;
            flashSpriteRenderer.color = new Color(1, 1, 1, 0);
        }

        else
        {
            currentFlickerTime += Time.deltaTime;

            if (currentFlickerTime >= flickerSpeed)
            {
                flashSpriteRenderer.color = flashSpriteRenderer.color.a == 0 ? new Color(0, 0, 0, 1) : new Color(0, 0, 0, 0);
                currentFlickerTime = 0f;
            }
        }
    }

}
