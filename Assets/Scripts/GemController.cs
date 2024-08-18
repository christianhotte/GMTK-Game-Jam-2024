using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GemController : MonoBehaviour
{
    //Settings:
    [SerializeField, Tooltip("")] private float magnetRadius = 1;
    [SerializeField, Tooltip("")] private float magnetStrength = 1;

    public void DestroyGem()
    {
        BoidShip boidShip = Instantiate(PlayerController.main.boidPrefab, transform.position, Quaternion.identity).GetComponent<BoidShip>();
        boidShip.active = true;
        PlayerController.main.ships.Add(boidShip);

        Destroy(gameObject);
    }
    private void Update()
    {
        BoidShip[] closeShips = Physics2D.OverlapCircleAll(transform.position, magnetRadius, LayerMask.GetMask("PlayerShip")).Select(o => o.GetComponent<BoidShip>()).ToArray();
        if (closeShips.Length > 0)
        {
            BoidShip closestShip = closeShips[0];
            float bestDist = Vector2.SqrMagnitude(transform.position - closestShip.transform.position);
            if (closeShips.Length > 1)
            {
                for (int x = 1; x < closeShips.Length; x++)
                {
                    float dist = Vector2.SqrMagnitude(transform.position - closeShips[x].transform.position);
                    if (dist < bestDist)
                    {
                        closestShip = closeShips[x];
                        bestDist = dist;
                    }
                }
            }
            float interpolant = 1 - Mathf.InverseLerp(0, magnetRadius, bestDist);
            Vector2 magnetAccel = interpolant * magnetStrength * Time.deltaTime * (closestShip.transform.position - transform.position).normalized;
            transform.position = transform.position + (Vector3)magnetAccel;
        }
    }
}
