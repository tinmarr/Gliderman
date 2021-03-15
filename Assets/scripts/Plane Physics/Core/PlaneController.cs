using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class PlaneController : MonoBehaviour
{
    [SerializeField]
    List<AeroSurface> controlSurfaces = null;
    [SerializeField]
    float rollControlSensitivity = 0.2f;
    [SerializeField]
    float pitchControlSensitivity = 0.2f;
    [SerializeField]
    float yawControlSensitivity = 0.2f;

    [Range(-1, 1)]
    public float Pitch;
    [Range(-1, 1)]
    public float Yaw;
    [Range(-1, 1)]
    public float Roll;
    [Range(-1, 1)]
    public float Flap;

    float thrustPercent;

    AircraftPhysics aircraftPhysics;
    Rigidbody rb;
    public ParticleSystem jet;
    public Text planeInfo;
    public GameObject cam;
    CinemachineVirtualCamera cinemachine;

    float terminalVelocity = 200f;

    private void Start()
    {
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();
        jet.Stop();
        cinemachine = cam.GetComponent<CinemachineVirtualCamera>();
    }

    private void Update()
    {
        Pitch = Input.GetAxis("Vertical");
        Roll = Input.GetAxis("Horizontal");
        Yaw = Input.GetAxis("Yaw");

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    setThrust(1f, 3);
        //}

        if (thrustPercent > 0.7f)
        {
            cinemachine.Priority = 3;
            CinemachineBasicMultiChannelPerlin noise = cinemachine.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            jet.Play();
        } else
        {
            cinemachine.Priority = 1;
            jet.Stop();
        }

        
        Vector3[] dirs = { transform.forward, -transform.forward, transform.up, -transform.up, transform.right, -transform.right };
        List<float> groundNear = new List<float>();
        foreach (Vector3 dir in dirs)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, dir, out hit, Mathf.Infinity))
            {
                groundNear.Add(hit.distance);
            } else
            {
                groundNear.Add(Mathf.Infinity);
            }
        }

        planeInfo.text = "V: " + (int)rb.velocity.magnitude + " m/s\nA: " + (int)transform.position.y + " m\nD: " + (int)Mathf.Min(groundNear.ToArray()) + " m";
        Debug.Log(groundNear.IndexOf(Mathf.Min(groundNear.ToArray())));

    }
        
    private void FixedUpdate()
    {
        SetControlSurfacesAngles(Pitch, Roll, Yaw, Flap);
        aircraftPhysics.SetThrustPercent(thrustPercent);
    }

    public void SetControlSurfacesAngles(float pitch, float roll, float yaw, float flap)
    {
        foreach (var surface in controlSurfaces)
        {
            if (surface == null || !surface.IsControlSurface) continue;
            switch (surface.InputType)
            {
                case ControlInputType.Pitch:
                    surface.SetFlapAngle(pitch * pitchControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Roll:
                    surface.SetFlapAngle(roll * rollControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Yaw:
                    surface.SetFlapAngle(yaw * yawControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Flap:
                    surface.SetFlapAngle(Flap * surface.InputMultiplyer);
                    break;
            }
        }
    }

    public void GetDistanceFromGround()
    {

    }

    public void SetThrust(float thrustPercent, int time = 0)
    {
        this.thrustPercent = thrustPercent;
        if (time != 0)
        {
            Invoke(nameof(ResetThrust), time);
        }
    }

    public void ResetThrust()
    {
        thrustPercent = 0;
    }
}
