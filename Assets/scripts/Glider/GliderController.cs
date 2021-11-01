using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class GliderController : MonoBehaviour
{
    [Header("Config")]
    public GliderControllerConfig config;

    [Header("Control Parameters")]
    [SerializeField]
    List<AeroSurface> controlSurfaces = null;
    readonly float[] sensitivitySaves = new float[3];
    public FlapController[] flaps;
    float[] flapAngles = { 0, 0, 0, 0 }; // top to bottom then left to right

    [Header("Debug Variables")]
    [Range(-2, 2)]
    public float Pitch;
    [Range(-2, 2)]
    public float Yaw;
    [Range(-2, 2)]
    public float Roll;
    [Range(-2, 2)]
    public float Flap;

    [Header("Physics")]
    AircraftPhysics aircraftPhysics;
    Rigidbody rb;

    [Header("Jet Parameters")]
    public ParticleSystem jet;
    float thrustPercent;
    bool speeding = false;
    bool jetEmpty = false;
    float fuelAmount = 0f;

    [Header("Trails")]
    public TrailRenderer rightTrail;
    public TrailRenderer leftTrail;
    public Material trailNormal;
    public Material trailBoost;
    public Material trailGround;

    [Header("Dampening Parameters")]
    public ControlDampener controlDampener;

    [Header("Camera")]
    public CinemachineVirtualCamera followCam;
    public CinemachineVirtualCamera followCamRoll;
    public CinemachineVirtualCamera shakeCam;
    public CinemachineVirtualCamera shakeCamRoll;
    CinemachineVirtualCamera currentCam;

    [Header("Brakes")]
    public AeroSurface brake;
    bool braking;

    bool dead = false;
    Vector3 startPos;
    Quaternion startRot;
    Vector3 startScale;
    bool launched = false;
    float[] groundNear = new float[1];
    float aliveSince = 0;

    [HideInInspector]
    public PlayerInput input;

    //("Game loop do not touch")
    bool doNothing = false;

    [Header("Score")]
    public int highScore = 0;
    public int lastScore = 0;
    public int currentScore = 0;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();

        input.actions["Respawn"].performed += _ => { dead = true; };
    }

    private void Start()
    {
        StartCoroutine(AddToScore());

        dead = false;
        jet.Stop();
        startPos = transform.position;
        startRot = transform.rotation;
        startScale = transform.localScale;
        
        rightTrail.emitting = false;
        leftTrail.emitting = false;
        sensitivitySaves[0] = config.rollControlSensitivity;
        sensitivitySaves[1] = config.pitchControlSensitivity;
        sensitivitySaves[2] = config.yawControlSensitivity;
        Respawn();
    }
    void CheckHeight()
    {
        if (transform.position.y < -1)
        {
            dead = true;
        }
    }

    public void SetAttitude()
    {
        Vector2 value = input.actions["Attitude"].ReadValue<Vector2>();

        Roll = value.x;
        Pitch = value.y;
        Yaw = input.actions["Yaw"].ReadValue<float>();

        controlDampener.Dampen(ref Pitch, ref Roll, rb.velocity.magnitude, config.terminalVelocity);
    }

    private void Update()
    {
        if (doNothing) return;

        SetAttitude();
        SetJet();

        Brake();

        CheckHeight();

        // Trails
        if ((int)rb.velocity.magnitude > 55)
        {
            rightTrail.material = trailNormal;
            leftTrail.material = trailNormal;
            rightTrail.emitting = true;
            leftTrail.emitting = true;
        }
        else
        {
            rightTrail.emitting = false;
            leftTrail.emitting = false;
        }

        // Jet
        if (config.fuelHack) fuelAmount = 2;
        if (fuelAmount <= 0)
        {
            jetEmpty = true;
        } else if (fuelAmount > 0.2f)
        {
            jetEmpty = false;
        }
        if (speeding)
        {
            jet.Play();
            rightTrail.material = trailBoost;
            leftTrail.material = trailBoost;
            rightTrail.emitting = true;
            leftTrail.emitting = true;
        }
        else
        {
            rightTrail.material = trailNormal;
            leftTrail.material = trailNormal;
            jet.Stop();
        }

        // Camera
        float anglex = transform.eulerAngles.x;
        if (anglex > 180) anglex -= 360;
        float abspitch = (anglex / 180);

        CinemachineVirtualCamera cam = followCam;
        currentCam = currentCam == null ? cam : currentCam;
        bool roll = Mathf.Abs(abspitch) > 0.3f;
        if (thrustPercent > 0.6f)
        {
            cam = roll ? shakeCamRoll : shakeCam;
        }
        else if (roll)
        {
            cam = followCamRoll;
        }

        if (currentCam != cam)
        {

            currentCam.Priority = 1;
            cam.Priority = 2;
            currentCam = cam;
        }

        // Get Distance from Terrain
        float maxSearchDistance = 500;
        Vector3[] dirs = { transform.forward, -transform.forward, transform.up, -transform.up, transform.right, -transform.right };
        groundNear = new float[dirs.Length];
        for (int i = 0; i < dirs.Length; i++)
        {
            Vector3 dir = dirs[i];
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, maxSearchDistance, 1 << 3))
            {
                groundNear[i] = (hit.distance);
            }
            else
            {
                groundNear[i] = maxSearchDistance;
            }
        }

        // Gain Boost
        float increaseValue = 0;
        float heightValue = 0;
        if (!speeding)
        {
            for (int i = 0; i < groundNear.Length; i++)
            {
                float toIncrease = config.proximityCurve.Evaluate(Mathf.InverseLerp(0, maxSearchDistance / 5, groundNear[i]));
                heightValue = Mathf.Max(toIncrease, heightValue);
                if (toIncrease > 0.25f)
                {
                    increaseValue += toIncrease;
                }
            }

            if (increaseValue > 0.25f) // Trails
            {
                fuelAmount += increaseValue * (1/(config.increaseTime*50)) * Time.deltaTime * (rb.velocity.magnitude/config.impactOfVelocity);
                rightTrail.emitting = true;
                leftTrail.emitting = true;
                rightTrail.material = trailGround;
                leftTrail.material = trailGround;
            }
            else
            {
                rightTrail.material = trailNormal;
                leftTrail.material = trailNormal;
            }
        }

        fuelAmount = Mathf.Clamp(fuelAmount, 0, 1);
        thrustPercent = Mathf.Clamp(thrustPercent, 0, 1);

        // Score
        if (Time.timeScale != 0)
        {
            currentScore += Mathf.RoundToInt(increaseValue > 0.5f ? increaseValue : 0);
        }

        // Death    
        if (dead)
        {
            thrustPercent = 0;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            jet.Stop();
        }

        // Update Visual Flaps
        for (int i = 0; i < flaps.Length; i++)
        {
            flaps[i].SetFlap(flapAngles[i]);
        }

        if (!launched) fuelAmount = 0;

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, config.terminalVelocity);
    }

    public void SetJet()
    {
        float boostAmount = input.actions["Boost"].ReadValue<float>();
        if (!jetEmpty && boostAmount > 0)
        {
            SetThrust(boostAmount, 0.1f);
            fuelAmount -= (1 / config.decreaseTime) * Time.deltaTime * boostAmount;
            speeding = true;
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
                    surface.SetFlapAngle(pitch * config.pitchControlSensitivity * surface.InputMultiplyer);
                    rightFlaps += pitch * config.pitchControlSensitivity * surface.InputMultiplyer;
                    leftFlaps += pitch * config.pitchControlSensitivity * surface.InputMultiplyer;
                    break;
                case ControlInputType.Roll:
                    surface.SetFlapAngle(roll * config.rollControlSensitivity * surface.InputMultiplyer);
                    if (surface.InputMultiplyer < 0) { leftFlaps += roll * config.rollControlSensitivity * surface.InputMultiplyer * 2; }
                    else if (surface.InputMultiplyer > 0) { rightFlaps += roll * config.rollControlSensitivity * surface.InputMultiplyer * 2; }
                    break;
                case ControlInputType.Yaw:
                    surface.SetFlapAngle(yaw * config.yawControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Flap:
                    surface.SetFlapAngle(Flap * surface.InputMultiplyer);
                    break;
            }
        }

        leftFlaps *= -300;
        rightFlaps *= -300;

        if (!braking)
        {
            for (int i = 0; i < flapAngles.Length; i++)
            {

                flapAngles[i] = Mathf.Lerp(flapAngles[i], i < flapAngles.Length / 2 ? leftFlaps : rightFlaps, config.closeSpeed);
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
        thrustPercent = 0;
    }

    public void Kill()
    {
        dead = true;

    }

    public void Revive()
    {
        dead = false;
    }


    public bool IsDead()
    {
        return dead;
    }

    public void Respawn()
    {
        lastScore = currentScore;
        if (lastScore > highScore) highScore = lastScore;
        currentScore = 0;
        transform.position = startPos;
        transform.rotation = startRot;
        transform.localScale = startScale;
        rb.constraints = RigidbodyConstraints.None;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        fuelAmount = 0f;
        launched = false;
        ResetThrust();
    }

    public void Brake()
    {
        float amount = Mathf.Clamp(input.actions["Brake"].ReadValue<float>(), (float)0.001, 1);
        
        brake.config.chord = amount;
        braking = amount >= 0.002;

        if (braking)
        {
            for (int i = 0; i < flapAngles.Length; i++)
            {
                flapAngles[i] = (i % 2) switch
                {
                    0 => Mathf.Lerp(flapAngles[i], 90 * amount, config.flapOpenSpeed),
                    _ => Mathf.Lerp(flapAngles[i], -90 * amount, config.flapOpenSpeed),
                };
            }

            config.rollControlSensitivity = sensitivitySaves[0] * 2f;
            config.pitchControlSensitivity = sensitivitySaves[1] * 2f;
        } else
        {
            config.rollControlSensitivity = sensitivitySaves[0];
            config.pitchControlSensitivity = sensitivitySaves[1];
        }
    }

    public Rigidbody GetRB()
    {
        return rb;
    }

    public void SetLaunched(bool val)
    {
        launched = val;
        if (launched == true) aliveSince = Time.time;
    }

    public float RemainingFuel() { return fuelAmount;  }

    public float GetMinDistance()
    {
        return Mathf.Min(groundNear);
    }

    public float GetAliveSince()
    {
        return aliveSince;
    }
    
    public bool GetLaunched()
    {
        return launched;
    }
    public void SetNothing(bool val)
    {
        doNothing = val;
    }
    public IEnumerator AddToScore()
    {
        yield return new WaitForSeconds(1f);
        if (launched)
        {
            currentScore += 1;
        }
        StartCoroutine(AddToScore());
    }
}