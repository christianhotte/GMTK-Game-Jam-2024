using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemController : MonoBehaviour
{
    public void DestroyGem()
    {
        BoidShip boidShip = Instantiate(PlayerController.main.boidPrefab, transform.position, Quaternion.identity).GetComponent<BoidShip>();
        boidShip.active = true;
        PlayerController.main.ships.Add(boidShip);

        Destroy(gameObject);
    }
}
