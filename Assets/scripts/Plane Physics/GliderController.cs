using System.Collections.Generic;
using System;
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
    public FlapController[] flaps;
    float[] flapAngles = { 0, 0, 0, 0}; // top to bottom then left to right

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

    [Header("Trails")]
    public TrailRenderer rightTrail;
    public TrailRenderer leftTrail;

    [Header("Dampening Parameters")]
    // dampening
    public float terminalVelocity = 200f;
    public ControlDampener controlDampener;

    [Header("UI")]
    public Text planeInfo;
    public GameObject ded;

    [Header("Camera")]
    public CinemachineVirtualCamera followCam;
    public CinemachineVirtualCamera followCamRoll;
    public CinemachineVirtualCamera shakeCam;
    public CinemachineVirtualCamera shakeCamRoll;
    CinemachineVirtualCamera currentCam;

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
        startPos = transform.position;
        startRot = transform.rotation;
        startScale = transform.localScale;
        automation = GetComponent<Automation>();
        rightTrail.emitting = false;
        leftTrail.emitting = false;
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

        // Restart
        if (Input.GetKey(KeyCode.Return))
        {
            Respawn();
        }

        // Jet
        if (Input.GetKey(KeyCode.Space))
        {
            SetThrust(1, 0.1f);
        }
        if (speeding)
        {
            jet.Play();
        } else
        {
            jet.Stop();
        }

        // Brakes
        Brake();

        // Camera
        float anglex = transform.eulerAngles.x;
        float anglez = transform.eulerAngles.z;
        if (anglex > 180) anglex -= 360;
        if (anglez > 180) anglez -= 360;
        float abspitch = (anglex / 180);

        CinemachineVirtualCamera cam = followCam;
        currentCam = currentCam == null ? cam : currentCam;
        bool roll = Mathf.Abs(abspitch) > 0.3f;
        if (thrustPercent > 0.6f)
        {
            cam = roll ? shakeCamRoll : shakeCam;
        } else if (roll)
        {
            cam = followCamRoll;
        }

        if (currentCam != cam)
        {
            
            currentCam.Priority = 1;
            cam.Priority = 2;
            currentCam = cam;
        }

        // Clamp Control
        automation.angleClamp = !roll;
        automation.autoCorrect = !roll;
        automation.autoTurn = !roll;
        
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
            thrustPercent = proximityCurve.Evaluate(Mathf.InverseLerp(0, 100, Mathf.Min(groundNear)));
            if (thrustPercent > 0.25f) // Trails
            {
                Vector3 closestDir = dirs[Array.IndexOf(groundNear, Mathf.Min(groundNear))];
                if (closestDir == transform.right) { rightTrail.emitting = true; leftTrail.emitting = false; }
                else if (closestDir == -transform.right) { leftTrail.emitting = true; rightTrail.emitting = false; }
                else { rightTrail.emitting = true; leftTrail.emitting = true; }
            }
            else
            {
                rightTrail.emitting = false;
                leftTrail.emitting = false;
            }
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
            "\nPitch: " + abspitch.ToString("n2")+ // Calculated above for the camera
            "\nD: " + (int) Mathf.Min(groundNear) + " m";

        // Flaps
        for (int i = 0; i < flaps.Length; i++)
        {
            flaps[i].SetFlap(flapAngles[i]);
        }
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
        float rightFlaps = 0;
        float leftFlaps = 0;
        foreach (var surface in controlSurfaces)
        {
            if (surface == null || !surface.IsControlSurface) continue;
            switch (surface.InputType)
            {
                case ControlInputType.Pitch:
                    surface.SetFlapAngle(pitch * pitchControlSensitivity * surface.InputMultiplyer);
                    rightFlaps += pitch * pitchControlSensitivity * surface.InputMultiplyer;
                    leftFlaps += pitch * pitchControlSensitivity * surface.InputMultiplyer;
                    break;
                case ControlInputType.Roll:
                    surface.SetFlapAngle(roll * rollControlSensitivity * surface.InputMultiplyer);
                    if (surface.InputMultiplyer > 0) { leftFlaps += roll * rollControlSensitivity * surface.InputMultiplyer * 2; }
                    else if (surface.InputMultiplyer < 0) { rightFlaps += roll * rollControlSensitivity * surface.InputMultiplyer * 2; }
                    break;
                case ControlInputType.Yaw:
                    surface.SetFlapAngle(yaw * yawControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Flap:
                    surface.SetFlapAngle(Flap * surface.InputMultiplyer);
                    break;
            }
        }

        leftFlaps *= -300;
        rightFlaps *= -300;
        for (int i = 0; i < flapAngles.Length; i++)
        {
            flapAngles[i] = i < flapAngles.Length / 2 ? leftFlaps : rightFlaps;
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
        ResetThrust();
    }

    public void Brake()
    {
        if (Input.GetKey(KeyCode.LeftShift) && rb.velocity.magnitude > minVelocity)
        {
            foreach (Transform brake in brakes)
            {
                brake.localRotation = Quaternion.Euler(brake.localEulerAngles.x, brake.localEulerAngles.y, 90);
            }

            for (int i = 0; i < flapAngles.Length; i++)
            {
                flapAngles[i] = (i % 2) switch
                {
                    0 => 90,
                    _ => -90,
                };
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