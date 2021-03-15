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

    // dampening
    float terminalVelocity = 200f;
    public ControlDampener controlDampener;

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

        controlDampener.DampenPitch(ref Pitch, ref Roll, rb.velocity.magnitude, terminalVelocity);

        if (thrustPercent > 0.6f)
        {
            cinemachine.Priority = 3;
            
        } else if (thrustPercent > 0.3f)
        {
            jet.Play();
        } else
        {
            cinemachine.Priority = 1;
            jet.Stop();
        }

        
        // Get Distance from Terrain
        Vector3[] dirs = { transform.forward, -transform.forward, transform.up, -transform.up, transform.right, -transform.right };
        float[] groundNear = new float[dirs.Length];
        for (int i=0; i<dirs.Length; i++)
        {
            Vector3 dir = dirs[i];
            RaycastHit hit;
            if (Physics.Raycast(transform.position, dir, out hit, Mathf.Infinity, 1 << 3)) // Mathf.Infinity is fine for now... might need to lessen that
            {
                groundNear[i] = (hit.distance);
            } else
            {
                groundNear[i] = Mathf.Infinity; // Also update here
            }
        }
        planeInfo.text = "V: " + (int)rb.velocity.magnitude + " m/s\nA: " + (int)transform.position.y + " m\nD: " + (int) Mathf.Min(groundNear) + " m";
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
    public float GetTerminalVelocity() { return terminalVelocity; }
}
