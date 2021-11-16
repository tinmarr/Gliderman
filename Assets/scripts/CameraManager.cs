using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    public PlayerInput input;

    public int onPriority = 1;
    public int offPriority = 0;

    public CameraInfo[] cameras;

    CameraInfo currentCam;
    int currentIndex = 0;
    CinemachineBasicMultiChannelPerlin noiseSettings;

    private void Start()
    {
        foreach (CameraInfo cam in cameras)
        {
            cam.cam.Priority = offPriority;
        }

        currentCam = cameras[currentIndex];
        noiseSettings = cameras[currentIndex].cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        currentCam.cam.DestroyCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        currentCam.cam.Priority = onPriority;

        input.actions["ViewSwitch"].performed += _ => { SwitchCamera(); };
    }

    private void Update()
    {
        if (input.actions["Boost"].ReadValue<float>() > 0.2f)
        {
            CinemachineBasicMultiChannelPerlin noise = currentCam.cam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            noise.m_NoiseProfile = noiseSettings.m_NoiseProfile;
            noise.m_AmplitudeGain = noiseSettings.m_AmplitudeGain;
            noise.m_FrequencyGain = noiseSettings.m_FrequencyGain;
        }
        else
        {
            currentCam.cam.DestroyCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    void SwitchCamera()
    {
        currentIndex++;
        if (currentIndex >= cameras.Length) currentIndex = 0;

        currentCam.cam.Priority = offPriority;

        currentCam = cameras[currentIndex];
        currentCam.cam.Priority = onPriority;
    }
}

public enum CamType { Chase, ThirdPerson, FirstPerson }

[System.Serializable]
public struct CameraInfo
{
    public CinemachineVirtualCamera cam;
    public string name;
    public CamType type;
}