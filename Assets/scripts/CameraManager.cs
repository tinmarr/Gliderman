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
    public HUDController hudController;

    CameraInfo currentCam;
    int currentIndex = 0;
    CinemachineBasicMultiChannelPerlin noiseSettings;

    [Range(0, 5)]
    public float maxShake = 2f;

    public Transform lookAt;
    public Transform playerTransform;

    private void Start()
    {
        noiseSettings = cameras[0].cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        for (int i=0; i<cameras.Length; i++)
        {
            cameras[i].cam.Priority = offPriority;
            cameras[i].noise = cameras[i].cam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            cameras[i].noise.m_NoiseProfile = noiseSettings.m_NoiseProfile;
            cameras[i].noise.m_AmplitudeGain = 0;
            cameras[i].noise.m_FrequencyGain = noiseSettings.m_FrequencyGain;
        }

        currentCam = cameras[currentIndex];
        currentCam.cam.Priority = onPriority;

        input.actions["ViewSwitch"].performed += _ => { SwitchCamera(); };
    }

    private void LateUpdate()
    {
        //Vector2 pan = input.actions["Pan"].ReadValue<Vector2>();
        //Debug.Log(currentCam.cam.transform);
        //currentCam.cam.transform.RotateAround(playerTransform.position, currentCam.cam.transform.up, 0.1f * pan.x);
        //currentCam.cam.transform.RotateAround(playerTransform.position, currentCam.cam.transform.right, 0.1f * pan.y);
        //Debug.Log(currentCam.cam.transform);

        //CinemachineTransposer transposer = currentCam.cam.GetCinemachineComponent<CinemachineTransposer>();
        //transposer.m_FollowOffset = currentCam.cam.transform.position;

        currentCam.noise.m_AmplitudeGain = Mathf.Lerp(0, maxShake, input.actions["Boost"].ReadValue<float>());
    }

    void SwitchCamera()
    {
        currentIndex++;
        if (currentIndex >= cameras.Length) currentIndex = 0;

        currentCam.cam.Priority = offPriority;

        currentCam = cameras[currentIndex];
        currentCam.cam.Priority = onPriority;

        hudController.AddMessage(currentCam.name + " Active");
    }
}

public enum CamType { Chase, ThirdPerson, FirstPerson }

[System.Serializable]
public struct CameraInfo
{
    public CinemachineVirtualCamera cam;
    public string name;
    public CamType type;
    public CinemachineBasicMultiChannelPerlin noise;
}