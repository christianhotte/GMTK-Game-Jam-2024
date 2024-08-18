using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public float relativeScale;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }
    private void Update()
    {
        transform.localScale = relativeScale * PlayerController.main.cam.m_Lens.OrthographicSize * Vector3.one;

        Vector2 newPos = (Vector2)Camera.main.ScreenToWorldPoint(PlayerController.main.mousePosition);
        transform.position = newPos;
        float direction = Vector2.SignedAngle(Vector2.up, newPos - (Vector2)PlayerController.main.transform.position);
        transform.eulerAngles = Vector3.forward * direction;
    }
}
