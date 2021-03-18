using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class GliderController : MonoBehaviour
{
    [Header("Control Parameters")]
    [SerializeField]
    List<AeroSurface> controlSurfaces = null;
    [SerializeField]
    float rollControlSensitivity = 0.2f;
    [SerializeField]
    float pitchControlSensitivity = 0.2f;
    [SerializeField]
    float yawControlSensitivity = 0.2f;

    [Header("Display Variables")]
    [Range(-1, 1)]
    public float Pitch;
    [Range(-1, 1)]
    public float Yaw;
    [Range(-1, 1)]
    public float Roll;
    [Range(-1, 1)]
    public float Flap;
    [Tooltip("Toggled by shift, this helps you do the stuffs")]
    public bool noobSettings;

    [Header("Jet Parameters")]
    public float thrustPercent;
    AircraftPhysics aircraftPhysics;
    Rigidbody rb;
    public ParticleSystem jet;
    public AnimationCurve proximityCurve;
    bool speeding = false;

    [Header("Dampening Parameters")]
    // dampening
    public float terminalVelocity = 200f;
    public ControlDampener controlDampener;

    [Header("UI")]
    public Text planeInfo;
    public GameObject ded;

    [Header("Camera")]
    public GameObject cam;
    CinemachineVirtualCamera cinemachine;

    [Header("Brakes")]
    public Transform[] brakes = new Transform[2];
    public float minVelocity = 30;

    [Header("Other")]
    bool dead = false;
    Vector3 startPos;
    Quaternion startRot;
    Vector3 startScale;
    Automation automation;

    private void Start()
    {
        dead = false;
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();
        jet.Stop();
        cinemachine = cam.GetComponent<CinemachineVirtualCamera>();
        startPos = transform.position;
        startRot = transform.rotation;
        startScale = transform.localScale;
        automation = GetComponent<Automation>();
    }

    private void Update()
    {
        Pitch = Input.GetAxis("Vertical");
        Roll = Input.GetAxis("Horizontal");
        Yaw = 0;

        // Automation
        HandleNoob();
        if (noobSettings) automation.NoobSettings(ref Pitch, ref Yaw, ref Roll);

        controlDampener.Dampen(ref Pitch, ref Roll, rb.velocity.magnitude, terminalVelocity);

        // Jet
        if (Input.GetKey(KeyCode.Space))
        {
            thrustPercent = 1;
        }

        // Brakes
        Brake();

        // Camera
        if (thrustPercent > 0.6f)
        {
            cinemachine.Priority = 3;
            jet.Play();
            
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

        // Boost
        if (!speeding)
        {
            thrustPercent += proximityCurve.Evaluate(Mathf.InverseLerp(0, 100, Mathf.Min(groundNear))) * Time.deltaTime;
        }

        thrustPercent = Mathf.Clamp(thrustPercent, 0, 1);

        // Death
        if (dead)
        {
            thrustPercent = 0;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            ded.SetActive(true);
        }
        // HUD
        planeInfo.text = "V: " + (int)rb.velocity.magnitude + " m/s"+
            "\nA: " + (int)transform.position.y + " m"+
            "\nT: " + (int) (thrustPercent * 100) + "%"+
            "\nPitch: " + (2*Mathf.InverseLerp(0, 360, transform.eulerAngles.x) - 1).ToString("n2");
    }

    private void FixedUpdate()
    {
        if (!dead)
        {
            SetControlSurfacesAngles(Pitch, Roll, Yaw, Flap);
            aircraftPhysics.SetThrustPercent(thrustPercent);
        }

    }

    private void HandleNoob()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            noobSettings = !noobSettings;
        }
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

    public void SetThrust(float thrustPercent, float time = 0)
    {
        this.thrustPercent = thrustPercent;
        if (time != 0)
        {
            speeding = true;
            Invoke(nameof(ResetThrust), time);
        }
    }

    public void ResetThrust()
    {
        speeding = false;
    }

    public float GetTerminalVelocity() { return terminalVelocity; }
    
    public void Kill()
    {
        dead = true;
    }


    public bool IsDead()
    {
        return dead;
    }

    public void Respawn()
    {
        dead = false;
        ded.SetActive(false);
        transform.position = startPos;
        transform.rotation = startRot;
        transform.localScale = startScale;
        rb.constraints = RigidbodyConstraints.None;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void Brake()
    {
        if (Input.GetKey(KeyCode.LeftShift) && rb.velocity.magnitude > minVelocity)
        {
            foreach (Transform brake in brakes)
            {
                brake.localRotation = Quaternion.Euler(brake.localEulerAngles.x, brake.localEulerAngles.y, 90);
            }
        } else
        {
            foreach (Transform brake in brakes)
            {
                brake.localRotation = Quaternion.Euler(brake.localEulerAngles.x, brake.localEulerAngles.y, 0);
            }
        }
    }
}