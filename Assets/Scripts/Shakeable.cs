using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shakeable : MonoBehaviour
{
    public static void ShakeTransform(Transform transform, float intensity, float frequency)
    {
        // Use Perlin noise for smooth random movement in each axis
        float offsetX = (Mathf.PerlinNoise(Time.time * 10f * frequency, 0f) - 0.5f) * 2f * intensity;
        float offsetY = (Mathf.PerlinNoise(Time.time * 10f * frequency, 100f) - 0.5f) * 2f * intensity;
        float offsetZ = (Mathf.PerlinNoise(Time.time * 10f * frequency, 200f) - 0.5f) * 2f * intensity;

        // Apply the shake offset
        transform.localPosition = new Vector3(offsetX, offsetY, offsetZ);
    }

    public static void ShakeTransform(RectTransform transform, float intensity, float frequency)
    {
        // Use Perlin noise for smooth random movement in each axis
        float offsetX = (Mathf.PerlinNoise(Time.time * 10f * frequency, 0f) - 0.5f) * 2f * intensity;
        float offsetY = (Mathf.PerlinNoise(Time.time * 10f * frequency, 100f) - 0.5f) * 2f * intensity;
        float offsetZ = (Mathf.PerlinNoise(Time.time * 10f * frequency, 200f) - 0.5f) * 2f * intensity;

        // Apply the shake offset
        transform.anchoredPosition = new Vector3(offsetX, offsetY, offsetZ);
    }
}
