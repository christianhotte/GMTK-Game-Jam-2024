using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveLinear : MonoBehaviour
{
    float lifetime = 5f;

    private void Update()
    {
        float distance = 1.5f * Time.deltaTime;
        Vector3 moveVector = new Vector3(distance, -distance, 0);
        transform.Translate(moveVector);

        lifetime -= Time.deltaTime;
        if (lifetime <= 0) { Destroy(gameObject); }
    }
}
