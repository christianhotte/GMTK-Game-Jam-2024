using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonController : MonoBehaviour
{
    [SerializeField, Tooltip("The number of ships to house in the prison.")] private int numberOfShips;
    [SerializeField, Tooltip("The container for the ships.")] private Transform shipParent;
    [SerializeField, Tooltip("The health of the prison cell.")] private float health;
    [SerializeField, Tooltip("The container for the prison visuals.")] private Transform prisonVisualParent;
    [SerializeField, Tooltip("The intensity of the damage shake.")] private float shakeIntensity;
    [SerializeField, Tooltip("The duration of the damage shake.")] private float shakeDuration;
    [SerializeField, Tooltip("The duration of the damage shake.")] private float shakeFrequency = 1;
    [SerializeField, Tooltip("The rotation speed of the prison.")] private float rotationSpeed;

    public bool debugDamage = false;

    private float currentHealth;
    private bool isShaking;
    private float currentShakeTime;

    private List<BoidShip> boidShips;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        boidShips = new List<BoidShip>(numberOfShips);

        for (int i = 0; i < numberOfShips; i++)
        {
            BoidShip boidShip = Instantiate(PlayerController.main.boidPrefab, shipParent).GetComponent<BoidShip>();
            boidShips.Add(boidShip);
        }

        currentHealth = health;
    }

    public void DamagePrison(float damage)
    {
        currentHealth -= damage;
        isShaking = true;
        currentShakeTime = 0f;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            PlayerController.main.ships.AddRange(boidShips);

            for(int i = 0; i < numberOfShips; i++)
                boidShips[i].transform.SetParent(null);

            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (debugDamage)
        {
            debugDamage = false;
            DamagePrison(10);
        }

        if (isShaking)
            ShakePrison();

        prisonVisualParent.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
    }

    private void ShakePrison()
    {
        currentShakeTime += Time.deltaTime;

        Shakeable.ShakeTransform(prisonVisualParent, shakeIntensity, shakeFrequency);

        if(currentShakeTime >= shakeDuration)
        {
            isShaking = false;
            prisonVisualParent.localPosition = Vector2.zero;
        }
    }
}
