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
    [Min(0)] public float maxBoidSpeed;
    [Min(0)] public float maxBoidForce;
    [Min(0)] public float boidRotRate;
    [Space()]
    [Min(0)] public float boidNeighborRadius;
    [Min(0)] public float boidSeparationRadius;
    [Min(0)] public float boidSepForce;
    [Min(0)] public float boidCohForce;
    [Min(0)] public float boidAlignForce;
    [Space()]
    public float boidLeaderFollowForce;
    public Vector2 boidLeaderSepRadii;
    [Range(0, 1)] public float leaderAlignBlend;
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

        foreach (BoidShip boid in ships)
        {
            boid.transform.position = boid.transform.position + ((Vector3)boid.velocity * Time.deltaTime);

            float angleTarget = Mathf.LerpAngle(Vector2.SignedAngle(Vector2.up, boid.velocity), Vector2.SignedAngle(Vector2.up, transform.up), leaderAlignBlend);
            boid.transform.eulerAngles = Vector3.forward * Mathf.LerpAngle(boid.transform.eulerAngles.z, angleTarget, boidRotRate * Time.deltaTime);
        }
    }
    private void FixedUpdate()
    {
        //Flock ships:
        List<Vector2> newVelocities = new List<Vector2>();
        foreach (BoidShip boid in ships)
        {
            List<BoidShip> neighbors = Physics2D.OverlapCircleAll(boid.transform.position, boidNeighborRadius).Where(b => b.TryGetComponent(out BoidShip shipCont) && shipCont != boid).Select(b => b.GetComponent<BoidShip>()).ToList();
            Vector2 newBoidVel = boid.velocity;

            //Neighbor velocity checks:
            if (neighbors.Count > 0)
            {

                //Apply neighbor alignment:
                /*Vector2 alignVelocity = Vector2.zero;
                foreach (BoidShip neighbor in neighbors)
                {
                    alignVelocity += neighbor.velocity;
                }
                alignVelocity /= neighbors.Count;
                alignVelocity *= maxBoidForce * boidAlignForce;
                alignVelocity = LimitMagnitude(alignVelocity, maxBoidSpeed);
                newBoidVel += alignVelocity;*/

                //Apply neighbor cohesion:
                Vector2 cohesionVelocity = Vector2.zero;
                Vector2 totalPosition = Vector2.zero;
                foreach (BoidShip neighbor in neighbors)
                {
                    totalPosition += (Vector2)neighbor.transform.position;
                }
                totalPosition /= neighbors.Count;
                Vector2 cohesionSep = (Vector2)boid.transform.position - totalPosition;
                cohesionVelocity = -boidCohForce * maxBoidForce * cohesionSep;
                cohesionVelocity = LimitMagnitude(cohesionVelocity, maxBoidSpeed);
                newBoidVel += cohesionVelocity;

                //Apply neighbor separation:
                Vector2 separationVelocity = Vector2.zero;
                int separatorNeighbors = 0;
                foreach (BoidShip neighbor in neighbors)
                {
                    Vector2 separation = boid.transform.position - neighbor.transform.position;
                    float sepDist = separation.magnitude;
                    if (sepDist < boidSeparationRadius)
                    {
                        separatorNeighbors++;
                        float sepStrength = Mathf.Clamp01(Mathf.InverseLerp(0, boidSeparationRadius, sepDist));
                        separationVelocity += maxBoidForce * boidSepForce * sepStrength * separation.normalized;
                    }
                }
                if (separatorNeighbors > 0) separationVelocity /= separatorNeighbors;
                separationVelocity = LimitMagnitude(separationVelocity, maxBoidSpeed);
                newBoidVel += separationVelocity;
            }

            //Leader velocity check:
            Vector2 leaderSeparation = transform.position - boid.transform.position;
            float leaderSepDist = leaderSeparation.magnitude;
            if (leaderSepDist > boidLeaderSepRadii.x)
            {
                float sepStrength = Mathf.Clamp01(Mathf.InverseLerp(boidLeaderSepRadii.x, boidLeaderSepRadii.y, leaderSepDist));
                Vector2 followVelocity = maxBoidForce * boidLeaderFollowForce * sepStrength * leaderSeparation.normalized;
                followVelocity = LimitMagnitude(followVelocity, maxBoidSpeed);
                newBoidVel += followVelocity;
            }

            //Cleanup:
            newVelocities.Add(newBoidVel);
        }
        for (int x = 0; x < ships.Count; x++)
        {
            BoidShip boid = ships[x];
            boid.velocity = LimitMagnitude(newVelocities[x], maxBoidSpeed);
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
