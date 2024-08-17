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

    //Settings:
    [Header("Look Settings:")]
    public float mouseSensitivityX = 1;
    public float mouseSensitivityY = 1;
    public float maxHeadUpAngle = 90;
    public float maxHeadDownAngle = -90;

    //Runtime variables:
    private float cameraPitch;

    //UNITY METHODS:
    private void Awake()
    {
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
        }
    }

    //INPUT METHODS:
    public void OnMouse(InputAction.CallbackContext ctx)
    {
        Vector2 mouseValue = ctx.ReadValue<Vector2>();
        Quaternion bodyRotation = transform.rotation;
        bodyRotation *= Quaternion.AngleAxis(mouseValue.x * mouseSensitivityX, Vector3.up);
        transform.rotation = bodyRotation;
        
        cameraPitch -= mouseValue.y * mouseSensitivityY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxHeadDownAngle, maxHeadUpAngle);
        cam.transform.localEulerAngles = Vector3.right * cameraPitch;
    }
}
