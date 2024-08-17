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

    //Settings:
    [Header("Look Settings:")]
    public float mouseSensitivityX = 1;
    public float mouseSensitivityY = 1;
    public float maxHeadUpAngle = 90;
    public float maxHeadDownAngle = -90;
    [Header("Movement Settings:")]
    public float moveSpeed = 1;
    [Header("Lift Settings:")]
    public float liftDistance = 1;
    public float liftWeight = 1;

    //Runtime variables:
    private float cameraPitch;
    private Vector2 moveValue;
    private bool lifting;
    [SerializeField] private float activeScale = 1;
    private Liftable currentLiftable;

    //UNITY METHODS:
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>();
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
            case "Move": OnMove(ctx); break;
            case "Lift": OnLift(ctx); break;
            default: break;
        }
    }
    private void Update()
    {
        transform.Translate(new Vector3(moveValue.x, 0, moveValue.y) * Time.deltaTime * activeScale);

        //Check for object:
        if (!lifting)
        {
            Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, liftDistance * activeScale, LayerMask.GetMask("Liftable"));
            if (hit.collider != null)
            {
                currentLiftable = hit.collider.GetComponentInParent<Liftable>();
                if (currentLiftable != null)
                {

                }
            }
        }
    }

    //INPUT METHODS:
    private void OnMouse(InputAction.CallbackContext ctx)
    {
        Vector2 mouseValue = ctx.ReadValue<Vector2>();
        Quaternion bodyRotation = transform.rotation;
        bodyRotation *= Quaternion.AngleAxis(mouseValue.x * mouseSensitivityX, Vector3.up);
        transform.rotation = bodyRotation;
        
        cameraPitch -= mouseValue.y * mouseSensitivityY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxHeadDownAngle, maxHeadUpAngle);
        cam.transform.localEulerAngles = Vector3.right * cameraPitch;
    }
    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveValue = ctx.ReadValue<Vector2>();
        moveValue = Quaternion.AngleAxis(transform.rotation.z, Vector3.up) * moveValue;
        moveValue = moveValue.normalized * moveSpeed;
    }
    private void OnLift(InputAction.CallbackContext ctx)
    {
        //Validity checks:
        if (lifting) return;
        if (currentLiftable == null) return;
    }

    //UTILITY METHODS:
    private void SetScale(float newScale)
    {
        transform.localScale = Vector3.one * newScale;
        activeScale = newScale;
    }
}
