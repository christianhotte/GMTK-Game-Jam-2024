using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    //Objects & Components:
    public static PlayerController main;
    public CinemachineVirtualCamera cam;
    private PlayerInput input;
    private InputActionMap inputMap;
    public GameObject projectilePrefab;
    public GameObject boidPrefab;
    internal List<BoidShip> ships = new List<BoidShip>();
    public BoidSettings[] boidSettingsList;
    private BoidSettings boidSettings;

    //Settings:
    [Header("Movement Settings:")]
    public float thrustPower;
    public float dragCoefficient;
    public float maxSpeed;
    public float rotationRate;
    [Header("Other Settings:")]
    public float cameraAdjustRate = 1;
    [Header("Boid Settings:")]
    [Min(0)] public float maxBoidSpeed;
    [Min(0)] public float maxBoidForce;
    [Min(0)] public float boidRotRate;
    [Min(0)] public float boidDragCoefficient;
    [Space()]
    [Min(0)] public float boidNeighborRadius;
    [Min(0)] public float boidSeparationRadius;
    [Min(0)] public float boidSepForce;
    [Min(0)] public float boidCohForce;
    [Min(0)] public float boidAlignForce;
    [Space()]
    public float boidLeaderFollowForce;
    [Range(0, 1)] public float boidLeaderVelocityInheritance;
    public Vector2 boidLeaderSepRadii;
    [Range(0, 1)] public float leaderAlignBlend;
    [Space()]
    public bool drawBoidRadii;

    //Runtime variables:
    private Vector2 mousePosition;
    private bool thrusting;
    private bool firing;
    internal Vector2 velocity;
    private float timeUntilShoot;
    private float targetCameraSize;
    private int lastFireShip = 0;

    //UNITY METHODS:
    private void Awake()
    {
        main = this;
        input = GetComponent<PlayerInput>();
        inputMap = input.actions.FindActionMap("ActionMap");
        inputMap.actionTriggered += OnPlayerInput;
        boidSettings = (BoidSettings)ScriptableObject.CreateInstance("BoidSettings");
        UpdateBoidSettings();
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
            case "DebugDeSpawn": OnDebugDeSpawn(ctx); break;
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
                Gizmos.DrawWireSphere(boid.transform.position, boidSettings.boidNeighborRadius);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(boid.transform.position, boidSettings.boidSeparationRadius);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, boidSettings.boidLeaderSepRadii.x);
                Gizmos.DrawWireSphere(transform.position, boidSettings.boidLeaderSepRadii.y);
            }
        }
    }
    private void Update()
    {
        //Rotate player:
        float targetRot = Vector3.SignedAngle(Vector3.up, (Vector2)Camera.main.ScreenToWorldPoint(mousePosition) - (Vector2)transform.position, Vector3.forward);
        float newRot = Mathf.LerpAngle(transform.eulerAngles.z, targetRot, rotationRate * Time.deltaTime);
        transform.eulerAngles = Vector3.forward * newRot;

        //Move player:
        if (thrusting) velocity += Time.deltaTime * thrustPower * (Vector2)transform.up; //Apply thrust
        velocity -= Time.deltaTime * dragCoefficient * velocity;                         //Apply drag
        if (velocity.magnitude > maxSpeed) velocity = velocity.normalized * maxSpeed;    //Cap speed

        Vector2 newPos = transform.position;
        newPos += velocity * Time.deltaTime;
        transform.position = newPos;

        //Move boids:
        foreach (BoidShip boid in ships)
        {
            if (thrusting) boid.velocity += Time.deltaTime * thrustPower * boidSettings.boidLeaderVelocityInheritance * (Vector2)transform.up;
            boid.velocity -= Time.deltaTime * boidSettings.boidDragCoefficient * boid.velocity; //Apply drag
            boid.transform.position = boid.transform.position + ((Vector3)boid.velocity * Time.deltaTime);

            float boidTargetRot = Vector3.SignedAngle(Vector3.up, (Vector2)Camera.main.ScreenToWorldPoint(mousePosition) - (Vector2)boid.transform.position, Vector3.forward);
            boidTargetRot = Mathf.LerpAngle(boid.transform.eulerAngles.z, boidTargetRot, boidSettings.boidRotRate * Time.deltaTime);
            boid.transform.eulerAngles = Vector3.forward * boidTargetRot;
        }

        //Fire weapons:
        if (firing)
        {
            timeUntilShoot -= Time.deltaTime;
            if (timeUntilShoot <= 0)
            {
                timeUntilShoot = 1 / boidSettings.fireRate; //Set time until shoot so that baseFireRate is in shots per second
                Fire();
            }
        }
    }
    private void FixedUpdate()
    {
        UpdateBoidSettings();
        //Flock ships:
        List<Vector2> newVelocities = new List<Vector2>();
        foreach (BoidShip boid in ships)
        {
            List<BoidShip> neighbors = Physics2D.OverlapCircleAll(boid.transform.position, boidSettings.boidNeighborRadius).Where(b => b.TryGetComponent(out BoidShip shipCont) && shipCont != boid).Select(b => b.GetComponent<BoidShip>()).ToList();
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
                cohesionVelocity = -boidSettings.boidCohForce * boidSettings.maxBoidForce * cohesionSep;
                cohesionVelocity = LimitMagnitude(cohesionVelocity, boidSettings.maxBoidSpeed);
                newBoidVel += cohesionVelocity;

                //Apply neighbor separation:
                Vector2 separationVelocity = Vector2.zero;
                int separatorNeighbors = 0;
                foreach (BoidShip neighbor in neighbors)
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
            BoidShip boid = ships[x];
            boid.velocity = LimitMagnitude(newVelocities[x], boidSettings.maxBoidSpeed);
        }

        //Update camera size:
        cam.m_Lens.OrthographicSize = Mathf.Lerp(cam.m_Lens.OrthographicSize, boidSettings.camSize, cameraAdjustRate);
    }

    //INPUT METHODS:
    private void OnMouse(InputAction.CallbackContext ctx)
    {
        Vector2 mouseValue = ctx.ReadValue<Vector2>();
        mousePosition = mouseValue;
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
            firing = true;
            timeUntilShoot = 0;
        }
        if (ctx.canceled) firing = false;
    }
    private void OnDebugSpawn(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            Transform newShip = Instantiate(boidPrefab.transform);
            ships.Add(newShip.GetComponent<BoidShip>());
            newShip.position = transform.position;
            UpdateBoidSettings();
        }
    }
    private void OnDebugDeSpawn(InputAction.CallbackContext ctx)
    {
        if (ctx.started && ships.Count > 0)
        {
            BoidShip shipToDestroy = ships[^1];
            ships.Remove(shipToDestroy);
            Destroy(shipToDestroy.gameObject);
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

    private void Fire()
    {
        //Increment barrel tracker:
        lastFireShip++;
        if (lastFireShip > ships.Count) lastFireShip = 0;

        //Find barrel:
        Transform barrel = null;
        if (lastFireShip == 0) barrel = transform;
        else barrel = ships[lastFireShip - 1].transform;

        //Fire projectile:
        ProjectileController newProj = Instantiate(projectilePrefab).GetComponent<ProjectileController>();
        newProj.Fire(barrel, barrel == transform ? velocity : ships[lastFireShip - 1].velocity);
    }
    public void UpdateBoidSettings()
    {
        int boidCount = ships.Count;
        if (boidCount <= boidSettingsList[0].boidNumber || boidSettingsList.Length == 1) boidSettingsList[0].CopyValuesTo(boidSettings);
        else
        {
            BoidSettings settingsA = boidSettingsList[0];
            foreach (BoidSettings s in boidSettingsList)
            {
                if (s.boidNumber == boidCount) { s.CopyValuesTo(boidSettings); return; }
                else if (s.boidNumber > boidCount)
                {
                    BoidSettings.LerpValues(settingsA, s, boidSettings, boidCount);
                    return;
                }
                settingsA = s;
            }
            boidSettingsList[^1].CopyValuesTo(boidSettings);
        }
    }
    public void IsHit()
    {
        if (ships.Count > 0)
        {
            //Find closest boid:
            BoidShip closestBoid = ships[0];
            float bestDist = Vector2.SqrMagnitude(closestBoid.transform.position - transform.position);
            if (ships.Count > 1)
            {
                foreach (BoidShip ship in ships)
                {
                    float shipDist = Vector2.SqrMagnitude(ship.transform.position - transform.position);
                    if (shipDist < bestDist)
                    {
                        closestBoid = ship;
                        bestDist = shipDist;
                    }
                }
            }

            //Swap places:
            Vector2 oldPos = transform.position;
            float oldRot = transform.eulerAngles.z;
            transform.position = closestBoid.transform.position;
            transform.rotation = closestBoid.transform.rotation;
            closestBoid.transform.position = oldPos;
            closestBoid.transform.eulerAngles = Vector3.forward * oldRot;

            //Destroy boid:
            ships.Remove(closestBoid);
            Destroy(closestBoid.gameObject);
        }
        else
        {
            //GAME OVER:
            GameManager.Instance.GameOver();
            Destroy(gameObject);
        }
    }
}
