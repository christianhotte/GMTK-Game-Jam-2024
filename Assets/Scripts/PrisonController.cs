using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonController : MonoBehaviour
{
    [SerializeField, Tooltip("The number of ships to house in the prison.")] private int numberOfShips;
    [SerializeField, Tooltip("The container for the ships.")] private Transform shipParent;
    [SerializeField, Tooltip("The health of the prison cell.")] private float health;
    [SerializeField, Tooltip("The container for the prison visuals.")] private Transform prisonVisualParent;
    public Transform barSpawnPoint;
    [SerializeField, Tooltip("The prefab used for bar visuals")] public GameObject bar;
    [SerializeField, Tooltip("The intensity of the damage shake.")] private float shakeIntensity;
    [SerializeField, Tooltip("The duration of the damage shake.")] private float shakeDuration;
    [SerializeField, Tooltip("The duration of the damage shake.")] private float shakeFrequency = 1;
    [SerializeField, Tooltip("The rotation speed of the prison.")] private float rotationSpeed;
    [SerializeField, Tooltip("The percentage of ships in the prison relative to the current ship count.")] private Vector2 shipPercentRange;
    [SerializeField, Tooltip("The maximum amount of ships to gain from a prison.")] private int maxShips = 30;
    [SerializeField, Tooltip("The random spawn radius for the boids after spawning.")] private float randomSpawnRadius;
    [SerializeField, Tooltip("The explosion particle for the prison.")] private GameObject explosionParticle;

    public bool debugDamage = false;

    private float currentHealth;
    private bool isShaking;
    private float currentShakeTime;
    private int totalShips;

    private List<BoidShip> boidShips;

    private void OnEnable()
    {
        Init();
        StartCoroutine(SpawnBar());
    }

    public void Init()
    {
        totalShips = numberOfShips;

        if(PlayerController.main.ships.Count > 0)
            totalShips += Mathf.CeilToInt(PlayerController.main.ships.Count * Random.Range(shipPercentRange.x, shipPercentRange.y));

        totalShips = Mathf.Clamp(totalShips, 0, maxShips);

        boidShips = new List<BoidShip>(totalShips);

        for (int i = 0; i < totalShips; i++)
        {
            BoidShip boidShip = Instantiate(PlayerController.main.boidPrefab, shipParent).GetComponent<BoidShip>();
            boidShip.active = false;
            boidShips.Add(boidShip);
        }

        currentHealth = health;
    }

    public void Damage(float damage)
    {
        currentHealth -= damage;
        isShaking = true;
        currentShakeTime = 0f;

        //PlaySound
        GameManager.Instance.AudioManager.PlayOneShot("Click", PlayerPrefs.GetFloat("AudioVolume", 0.5f), 0.7f, 1.3f);

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            for (int i = 0; i < totalShips; i++)
            {
                BoidShip newShip = Instantiate(PlayerController.main.boidPrefab).GetComponent<BoidShip>();
                newShip.transform.position = (Vector2)transform.position + (Random.insideUnitCircle * randomSpawnRadius);
                newShip.transform.eulerAngles = Vector2.Angle(Vector2.up, PlayerController.main.transform.position - transform.position) * Vector3.forward;
                PlayerController.main.ships.Add(newShip);
            }

            ScoreManager.Instance.AddToScore(totalShips);
            ScoreManager.Instance.AdjustShipNumber(PlayerController.main.ships.Count);
            GameManager.Instance.AudioManager.PlayOneShot("AsteroidExplode3", PlayerPrefs.GetFloat("AudioVolume", 0.5f));
            GameManager.Instance.AudioManager.PlayOneShot("BoidGet", PlayerPrefs.GetFloat("AudioVolume", 0.5f));
            Instantiate(explosionParticle, transform.position, Quaternion.identity);
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (debugDamage)
        {
            debugDamage = false;
            Damage(10);
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

    public IEnumerator SpawnBar()
    {
        var tempBar = Instantiate(bar, barSpawnPoint);
        yield return new WaitForSeconds(0.8f);
        StartCoroutine(SpawnBar());
    }
}
