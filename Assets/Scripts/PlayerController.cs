using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Objects & Components:
    private Camera cam;
    private PlayerInput input;
    private InputActionMap inputMap;
    private Rigidbody rb;
    public GameObject projectilePrefab;
    
    //Settings:
    [Header("Movement Settings:")]
    public float thrustPower;
    public float dragCoefficient;
    public float maxSpeed;
    public float rotationRate;

    //Runtime variables:
    private Vector2 mouseDirection;
    private bool thrusting;
    private Vector2 velocity;

    //UNITY METHODS:
    private void Awake()
    {
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
            default: break;
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
}
