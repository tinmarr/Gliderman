using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GliderController : MonoBehaviour
{
    [Header("Config")]
    public GliderControllerConfig config;
    GliderControllerConfig configCopy;

    [Header("Control Parameters")]
    [SerializeField]
    List<AeroSurface> controlSurfaces = null;
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
    [HideInInspector] public Rigidbody rb;

    [Header("Jet Parameters")]
    public ParticleSystem jet;
    float thrustPercent;
    bool speeding = false;
    bool jetEmpty = false;
    [HideInInspector] public float fuelAmount = 0f;

    [Header("Trails")]
    public TrailRenderer rightTrail;
    public TrailRenderer leftTrail;
    public Material trailNormal;
    public Material trailBoost;
    public Material trailGround;

    [Header("Brakes")]
    public AeroSurface brake;
    bool braking;

    [HideInInspector] public bool dead = false;
    Vector3 startPos;
    Quaternion startRot;
    float[] groundNear = new float[1];
    [HideInInspector] public float aliveSince = 0;


    [HideInInspector] public PlayerInput input;

    [Header("Score")]
    public int highScore = 0;
    public int lastScore = 0;
    public int currentScore = 0;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();

        config = config.clone();

        input.actions["Respawn"].performed += _ => { dead = true; };
    }

    private void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;

        dead = false;
        jet.Stop();
        
        rightTrail.emitting = false;
        leftTrail.emitting = false;
    }

    private void Update()
    {
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

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, config.terminalVelocity);
    }

    private void FixedUpdate()
    {
        SetControlSurfacesAngles(Pitch, Roll, Yaw, Flap);
        aircraftPhysics.SetThrustPercent(thrustPercent);
    }

    public void EnableAutoFly()
    {
        dead = false;
        rb.isKinematic = false;

        configCopy = config.clone();

        config.dragAmount = 0;

        rb.useGravity = false;

        rb.velocity = transform.forward * 50;
    }

    public void DisableAutoFly()
    {
        config = configCopy.clone();

        rb.useGravity = true;
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
        if (input.currentActionMap == input.actions.FindActionMap("Game"))
        {
            Vector2 value = input.actions["Attitude"].ReadValue<Vector2>();

            Roll = value.x;
            Pitch = value.y;
            Yaw = input.actions["Yaw"].ReadValue<float>();

            Pitch *= config.pitchCurve.Evaluate(Mathf.InverseLerp(0, config.terminalVelocity, rb.velocity.magnitude));
            Roll *= config.pitchCurve.Evaluate(Mathf.InverseLerp(0, config.terminalVelocity, rb.velocity.magnitude));
        }
        
    }

    public void SetJet()
    {
        if (input.currentActionMap == input.actions.FindActionMap("Game"))
        {
            float boostAmount = input.actions["Boost"].ReadValue<float>();
            if (!jetEmpty && boostAmount > 0)
            {
                SetThrust(boostAmount, 0.1f);
                fuelAmount -= (1 / config.decreaseTime) * Time.deltaTime * boostAmount;
                speeding = true;
            }
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
                    if (surface.InputMultiplyer > 0) { leftFlaps += roll * config.rollControlSensitivity * surface.InputMultiplyer * 2; }
                    else if (surface.InputMultiplyer < 0) { rightFlaps += roll * config.rollControlSensitivity * surface.InputMultiplyer * 2; }
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

    public void Respawn()
    {
        lastScore = currentScore;
        if (lastScore > highScore) highScore = lastScore;
        currentScore = 0;

        transform.rotation = startRot;
        transform.position = startPos;

        rb.constraints = RigidbodyConstraints.None;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Pitch = 0;
        Roll = 0;
        Yaw = 0;

        fuelAmount = 0f;
        dead = false;
        ResetThrust();
    }

    public void Brake()
    {
        float amount = Mathf.Clamp(input.actions["Brake"].ReadValue<float>(), config.dragAmount, 1);
        
        brake.config.chord = amount;
        braking = amount >= config.dragAmount + 1;

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
        }
    }

    public float GetMinDistance()
    {
        return Mathf.Min(groundNear);
    }

    public IEnumerator AddToScore()
    {
        yield return new WaitForSeconds(1f);
        currentScore += 1;
        StartCoroutine(AddToScore());
    }
}