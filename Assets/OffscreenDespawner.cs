using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffscreenDespawner : MonoBehaviour
{
    public float tickRate; //Rate at which the object checks for offscreen value
    private float tickTimer;

    public float offscreenLifetime; //How long the object can remain offscreen before it despawns
    private float offscreenTimer;

    public bool isOnscreen;

    private void OnEnable()
    {
        isOnscreen = true;
        tickTimer = tickRate;
        offscreenTimer = offscreenLifetime;
    }

    // Update is called once per frame
    void Update()
    {
        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0 || !isOnscreen)
        {
            tickTimer = tickRate;
            //Check Offscreen
            if (GameManager.Instance.IsOnScreen(transform.position) == false)
            {
                isOnscreen = false;
            }
            else { isOnscreen = true; }
        }

        if (!isOnscreen)
        {
            offscreenTimer -= Time.deltaTime;
        }
        else if (offscreenTimer != offscreenLifetime) { offscreenTimer = offscreenLifetime; }

        if (offscreenTimer <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
