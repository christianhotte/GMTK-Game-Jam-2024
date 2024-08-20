using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance;

    private CinemachineVirtualCamera virtualCamera;
    private float shakeTimer, shakeTimerTotal, startingCamIntensity;

    private CinemachineBasicMultiChannelPerlin cameraPerlin;

    private void Awake()
    {
        Instance = this;
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    private void Start()
    {
        cameraPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            cameraPerlin.m_AmplitudeGain = Mathf.Lerp(startingCamIntensity, 0f, 1 - (shakeTimer / shakeTimerTotal));
        }
        else
            cameraPerlin.m_AmplitudeGain = 0;
    }

    public void ShakeCamera(float intensity, float seconds)
    {
        cameraPerlin.m_AmplitudeGain = intensity;

        //Set values so that the shaking can eventually stop
        shakeTimer = seconds;
        shakeTimerTotal = seconds;
        startingCamIntensity = intensity;
    }
}
