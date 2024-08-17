using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    //Objects & Components:
    public static PlayerController main;
    private Camera cam;
    private PlayerInput input;
    private InputActionMap inputMap;
    private Rigidbody rb;
    public GameObject projectilePrefab;
    public GameObject boidPrefab;
    private List<BoidShip> ships = new List<BoidShip>();

    //Settings:
    [Header("Movement Settings:")]
    public float thrustPower;
    public float dragCoefficient;
    public float maxSpeed;
    public float rotationRate;
    [Header("Boid Settings:")]
    public float maxBoidSpeed;
    public float maxBoidForce;
    public float boidRotRate;
    [Space()]
    public float boidNeighborRadius;
    public float boidSeparationRadius;
    public float boidSepForce;
    public float boidCohForce;
    public float boidAlignForce;
    [Space()]
    public float boidLeaderFollowForce;
    public Vector2 boidLeaderSepRadii;
    [Space()]
    public bool drawBoidRadii;

    //Runtime variables:
    private Vector2 mouseDirection;
    private bool thrusting;
    private Vector2 velocity;

    //UNITY METHODS:
    private void Awake()
    {
        main = this;
        rb = GetComponent<Rigidbody>();
        input = GetComponent<PlayerInput>();
        inputMap = input.actions.FindActionMap("ActionMap");
        inputMap.actionTriggered += OnPlayerInput;
    }
    private void OnDisable()
    {
        inputMap.actionTriggered -= OnPlayerInput;
    }
    public void OnPlayerInput(InputAction.CallbackContext ctx)
    {
        switch (ctx.action.name)
        {
            case "Mouse": OnMouse(ctx); break;
            case "Thrust": OnThrust(ctx); break;
            case "Shoot": OnShoot(ctx); break;
            case "DebugSpawn": OnDebugSpawn(ctx); break;
            default: break;
        }
    }
    private void OnDrawGizmos()
    {
        foreach (BoidShip boid in ships)
        {
            if (drawBoidRadii)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(boid.transform.position, boidNeighborRadius);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(boid.transform.position, boidSeparationRadius);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, boidLeaderSepRadii.x);
                Gizmos.DrawWireSphere(transform.position, boidLeaderSepRadii.y);
            }
        }
    }
    private void Update()
    {
        //Rotate player:
        float targetRot = Vector3.SignedAngle(Vector3.up, mouseDirection, Vector3.forward);
        float newRot = Mathf.LerpAngle(transform.eulerAngles.z, targetRot, rotationRate * Time.deltaTime);
        transform.eulerAngles = Vector3.forward * newRot;

        //Move player:
        if (thrusting) velocity += Time.deltaTime * thrustPower * mouseDirection;     //Apply thrust
        velocity -= Time.deltaTime * dragCoefficient * velocity;                      //Apply drag
        if (velocity.magnitude > maxSpeed) velocity = velocity.normalized * maxSpeed; //Cap speed

        Vector2 newPos = transform.position;
        newPos += velocity;
        transform.position = newPos;

        //Flock ships:
        List<Vector2> newVelocities = new List<Vector2>();
        foreach (BoidShip boid in ships)
        {
            List<BoidShip> neighbors = Physics2D.OverlapCircleAll(boid.transform.position, boidNeighborRadius).Select(b => b.GetComponent<BoidShip>()).ToList();
            neighbors.Remove(boid);
            Vector2 newBoidVel = boid.velocity;

            //Neighbor velocity checks:
            if (neighbors.Count > 0)
            {
                //Gather data:
                Vector2 alignVelocity = Vector2.zero;
                Vector2 cohesionVelocity = Vector2.zero;
                Vector2 separationVelocity = Vector2.zero;
                Vector2 totalPositions = Vector2.zero;
                foreach (BoidShip neighbor in neighbors)
                {
                    alignVelocity += neighbor.velocity;
                    totalPositions += (Vector2)neighbor.transform.position;
                }
                /*
                //Apply neighbor alignment:
                alignVelocity /= neighbors.Count;
                alignVelocity = (alignVelocity.normalized * maxBoidSpeed) - boid.velocity;
                alignVelocity = LimitMagnitude(alignVelocity, maxBoidForce);
                newBoidVel += alignVelocity * boidAlignForce;

                //Apply neighbor cohesion:
                totalPositions /= neighbors.Count;
                cohesionVelocity = totalPositions - (Vector2)boid.transform.position;
                cohesionVelocity = (cohesionVelocity.normalized * maxBoidSpeed) - boid.velocity;
                cohesionVelocity = LimitMagnitude(cohesionVelocity, maxBoidForce);
                newBoidVel += cohesionVelocity * boidCohForce;*/

                //Apply neighbor separation:
                List<BoidShip> closeNeighbors = neighbors.Where(n => Vector2.Distance(n.transform.position, boid.transform.position) <= boidSeparationRadius / 2).ToList();
                foreach (BoidShip neighbor in closeNeighbors)
                {
                    Vector2 diff = (boid.transform.position - neighbor.transform.position);
                    separationVelocity += diff.normalized / diff.magnitude;
                }
                separationVelocity /= closeNeighbors.Count;
                separationVelocity = (separationVelocity.normalized * maxBoidSpeed) - boid.velocity;
                separationVelocity = LimitMagnitude(separationVelocity, maxBoidForce);
            }

            //Leader velocity check:
            Vector2 leaderSeparation = transform.position - boid.transform.position;
            float leaderSepDist = leaderSeparation.magnitude;
            if (leaderSepDist > boidLeaderSepRadii.x)
            {
                float sepStrength = Mathf.Clamp01(Mathf.InverseLerp(boidLeaderSepRadii.x, boidLeaderSepRadii.y, leaderSepDist));
                Vector2 followVelocity = maxBoidForce * sepStrength * leaderSeparation.normalized;
                newBoidVel += followVelocity;
            }

            //Cleanup:
            newVelocities.Add(newBoidVel);
        }
        for (int x = 0; x < ships.Count; x++)
        {
            BoidShip boid = ships[x];
            boid.velocity = LimitMagnitude(newVelocities[x], maxBoidSpeed);
            boid.transform.position = boid.transform.position + ((Vector3)boid.velocity * Time.deltaTime);
            boid.transform.eulerAngles = Vector3.forward * Mathf.LerpAngle(boid.transform.eulerAngles.z, Vector2.SignedAngle(Vector2.up, boid.velocity), boidRotRate * Time.deltaTime);
        }
    }

    //INPUT METHODS:
    private void OnMouse(InputAction.CallbackContext ctx)
    {
        Vector2 mouseValue = ctx.ReadValue<Vector2>();
        mouseDirection = (mouseValue - (Vector2)Camera.main.WorldToScreenPoint(transform.position)).normalized;
    }
    private void OnThrust(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) thrusting = true;
        if (ctx.canceled) thrusting = false;
    }
    private void OnShoot(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Transform newProj = Instantiate(projectilePrefab).transform;
            newProj.position = transform.position;
            newProj.rotation = transform.rotation;
        }
    }
    private void OnDebugSpawn(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            Transform newShip = Instantiate(boidPrefab.transform);
            ships.Add(newShip.GetComponent<BoidShip>());
            newShip.position = transform.position;
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

    public Vector2 GetVelocity() => velocity;
}
