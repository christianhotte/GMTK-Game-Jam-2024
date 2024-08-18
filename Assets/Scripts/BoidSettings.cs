using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/BoidSettings", order = 1)]
public class BoidSettings : ScriptableObject
{
    public int boidNumber;
    [Header("Boid Settings:")]
    [Min(0)] public float maxBoidSpeed;
    [Min(0)] public float maxBoidForce;
    [Min(0)] public float boidRotRate;
    [Min(0)] public float boidDragCoefficient;
    [Space()]
    [Min(0)] public float boidNeighborRadius;
    [Min(0)] public float boidSeparationRadius;
    [Min(0)] public float boidSepForce;
    [Min(0)] public float boidCohForce;
    [Min(0)] public float boidAlignForce;
    [Space()]
    public float boidLeaderFollowForce;
    [Range(0, 1)] public float boidLeaderVelocityInheritance;
    public Vector2 boidLeaderSepRadii;
    [Range(0, 1)] public float leaderAlignBlend;
    [Header("Other Settings:")]
    public float camSize;
    public float fireRate;
    [Header("Asteroid Settings:")]
    public Vector2 asteroidSizeRange;
    public float asteroidSpawnDistance;
    [Min(0)] public float asteroidSpawnRate;

    public void CopyValuesTo(BoidSettings target)
    {
        target.maxBoidSpeed = maxBoidSpeed;
        target.maxBoidForce = maxBoidForce;
        target.boidRotRate = boidRotRate;
        target.boidDragCoefficient = boidDragCoefficient;
        target.boidNeighborRadius = boidNeighborRadius;
        target.boidSeparationRadius = boidSeparationRadius;
        target.boidSepForce = boidSepForce;
        target.boidCohForce = boidCohForce;
        target.boidAlignForce = boidAlignForce;
        target.boidLeaderFollowForce = boidLeaderFollowForce;
        target.boidLeaderVelocityInheritance = boidLeaderVelocityInheritance;
        target.boidLeaderSepRadii = boidLeaderSepRadii;
        target.leaderAlignBlend = leaderAlignBlend;
        target.camSize = camSize;
        target.fireRate = fireRate;

        target.asteroidSizeRange = asteroidSizeRange;
        target.asteroidSpawnDistance = asteroidSpawnDistance;
        target.asteroidSpawnRate = asteroidSpawnRate;
    }
    public static void LerpValues(BoidSettings settingsA, BoidSettings settingsB, BoidSettings target, int boidCount)
    {
        float interpolant = Mathf.InverseLerp(settingsA.boidNumber, settingsB.boidNumber, boidCount);

        target.maxBoidSpeed = Mathf.Lerp(settingsA.maxBoidSpeed, settingsB.maxBoidSpeed, interpolant);
        target.maxBoidForce = Mathf.Lerp(settingsA.maxBoidForce, settingsB.maxBoidForce, interpolant);
        target.boidRotRate = Mathf.Lerp(settingsA.boidRotRate, settingsB.boidRotRate, interpolant);
        target.boidDragCoefficient = Mathf.Lerp(settingsA.boidDragCoefficient, settingsB.boidDragCoefficient, interpolant);
        target.boidNeighborRadius = Mathf.Lerp(settingsA.boidNeighborRadius, settingsB.boidNeighborRadius, interpolant);
        target.boidSeparationRadius = Mathf.Lerp(settingsA.boidSeparationRadius, settingsB.boidSeparationRadius, interpolant);
        target.boidSepForce = Mathf.Lerp(settingsA.boidSepForce, settingsB.boidSepForce, interpolant);
        target.boidCohForce = Mathf.Lerp(settingsA.boidCohForce, settingsB.boidCohForce, interpolant);
        target.boidAlignForce = Mathf.Lerp(settingsA.boidAlignForce, settingsB.boidAlignForce, interpolant);
        target.boidLeaderFollowForce = Mathf.Lerp(settingsA.boidLeaderFollowForce, settingsB.boidLeaderFollowForce, interpolant);
        target.boidLeaderVelocityInheritance = Mathf.Lerp(settingsA.boidLeaderVelocityInheritance, settingsB.boidLeaderVelocityInheritance, interpolant);
        target.boidLeaderSepRadii = Vector2.Lerp(settingsA.boidLeaderSepRadii, settingsB.boidLeaderSepRadii, interpolant);
        target.leaderAlignBlend = Mathf.Lerp(settingsA.leaderAlignBlend, settingsB.leaderAlignBlend, interpolant);
        target.camSize = Mathf.Lerp(settingsA.camSize, settingsB.camSize, interpolant);
        target.fireRate = Mathf.Lerp(settingsA.fireRate, settingsB.fireRate, interpolant);

        target.asteroidSizeRange = Vector2.Lerp(settingsA.asteroidSizeRange, settingsB.asteroidSizeRange, interpolant);
        target.asteroidSpawnDistance = Mathf.Lerp(settingsA.asteroidSpawnDistance, settingsB.asteroidSpawnDistance, interpolant);
        target.asteroidSpawnRate = Mathf.Lerp(settingsA.asteroidSpawnRate, settingsB.asteroidSpawnRate, interpolant);
    }
}
