using UnityEngine;

public class Glider2 : MonoBehaviour
{
    public float gravity = -9.8f;
    public float liftMultiplier = 1f;

    public float mass = 30f;
    public float wingArea = 20f;

    public float areaDensityConstant = 1.225f;

    public float rotationSpeed = 5f;
    public float jetSpeed = 10f;

    public float flapAngle = 0f;

    public float liftSlope = 6.28f;
    public float skinFriction = 0.02f;
    public float zeroLiftAoA = 0;
    public float stallAngleHigh = 15;
    public float stallAngleLow = -15;
    public float chord = 1;
    public float flapFraction = 0;
    public float span = 1;
    public bool autoAspectRatio = true;
    public float aspectRatio = 2;

    public ParticleSystem jet;

    private CharacterController controller;

    float yaw;
    float tilt;
    float roll;
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        jet.Stop();
    }

    private void Update()
    {
        float anglefromground = Vector3.Angle(Vector3.forward, transform.forward);
        float activeWingArea = Mathf.Cos(Mathf.Deg2Rad * anglefromground);
        GetInputs();
        AdjustRotations();
        ApplyPhysics();
        Jets();
    }

    private void ApplyPhysics()
    {
        Vector3 gravityForce = new Vector3(0, mass * gravity, 0);

        float correctedLiftSlope = liftSlope * aspectRatio /
           (aspectRatio + 2 * (aspectRatio + 4) / (aspectRatio + 2));

        // Calculating flap deflection influence on zero lift angle of attack
        // and angles at which stall happens.
        float theta = Mathf.Acos(2 * flapFraction - 1);
        float flapEffectivness = 1 - (theta - Mathf.Sin(theta)) / Mathf.PI;
        float deltaLift = correctedLiftSlope * flapEffectivness * FlapEffectivnessCorrection(flapAngle) * flapAngle;

        float zeroLiftAoaBase = zeroLiftAoA * Mathf.Deg2Rad;
        float newZeroLiftAoA = zeroLiftAoaBase - deltaLift / correctedLiftSlope;

        float stallAngleHighBase = stallAngleHigh * Mathf.Deg2Rad;
        float stallAngleLowBase = stallAngleLow * Mathf.Deg2Rad;

        float clMaxHigh = correctedLiftSlope * (stallAngleHighBase - zeroLiftAoaBase) + deltaLift * LiftCoefficientMaxFraction(flapFraction);
        float clMaxLow = correctedLiftSlope * (stallAngleLowBase - zeroLiftAoaBase) + deltaLift * LiftCoefficientMaxFraction(flapFraction);

        float newStallAngleHigh = newZeroLiftAoA + clMaxHigh / correctedLiftSlope;
        float newStallAngleLow = newZeroLiftAoA + clMaxLow / correctedLiftSlope;

        Vector3 airVelocity = transform.InverseTransformDirection(-controller.velocity);
        airVelocity = new Vector3(airVelocity.x, airVelocity.y);

        Vector3 dragDirection = transform.TransformDirection(airVelocity.normalized);
        Vector3 liftDirection = Vector3.Cross(dragDirection, transform.forward);

        float dynamicPressure = 0.5f * AirDensity(transform.position.y) * airVelocity.sqrMagnitude;
        float angleOfAttack = Mathf.Atan2(airVelocity.y, -airVelocity.x);

        Vector3 aerodynamicCoefficients = CalculateCoefficients(angleOfAttack,
                                                                correctedLiftSlope,
                                                                newZeroLiftAoA,
                                                                newStallAngleHigh,
                                                                newStallAngleLow);

        Vector3 lift = liftDirection * aerodynamicCoefficients.x * dynamicPressure * wingArea;
        Vector3 drag = dragDirection * aerodynamicCoefficients.y * dynamicPressure * wingArea;
        Vector3 torque = -transform.forward * aerodynamicCoefficients.z * dynamicPressure * wingArea * chord;

        Vector3 netAccel = (gravityForce + lift + drag + torque) / mass;
        Vector3 newVelocity = controller.velocity + netAccel * Time.deltaTime;
        Debug.Log(newVelocity);
        controller.Move(newVelocity * Time.deltaTime);
    }

    private float AirDensity(float altitude)
    {
        return areaDensityConstant;
    }

    private void GetInputs()
    {
        // turn body from forward to backward
        yaw = Input.GetAxis("Horizontal") * rotationSpeed / Time.timeScale;
        // go up or down
        tilt = Input.GetAxis("Vertical") * rotationSpeed / Time.timeScale;

        // roll cant really be adjusted by player
        // roll is if we had a string from top to bottom adjustment
        roll = 0;

        // if your world.up is | and transform.right is -- then the we havent turned our body
        // and the magnitude of that vector would be exactly 1.414214
        // the dot product is shorter if the angles are farther apart
        float tip = Vector3.Dot(transform.right, Vector3.up);// or we could just get the dot product
        //float tip = (transform.right + Vector3.up).magnitude - 1.414214f;
        roll -= tip;
        //float tip = Vector3.Dot(transform.right, Vector3.up); <-- find out about that

    }
    private void AdjustRotations()
    {
        //rotate yourself around the anchor point of your side down and up
        if (tilt != 0)
        {
            transform.Rotate(transform.right, tilt * Time.deltaTime, Space.World);
        }
        //rotate yourself around the anchor point of your forward to the right and left
        if (yaw != 0)
        {
            transform.Rotate(transform.forward, yaw * Time.deltaTime * -1, Space.World);
        }

        // --------------------- AUTO CORRECT ROTATION CODE --------------------------- //
        else
        {
            if (359 > transform.eulerAngles.z || transform.eulerAngles.z > 1)
            {
                yaw = (transform.eulerAngles.z < 60) ? 1 : -1;
                transform.Rotate(transform.forward, yaw * Time.deltaTime * -1, Space.World);
            }
        }

        if (roll != 0)
            transform.Rotate(Vector3.up, roll * Time.deltaTime * rotationSpeed, Space.World);

        if (transform.rotation.eulerAngles.z > 60 && transform.rotation.eulerAngles.z < 300)
        {
            int turnAngle;
            if (transform.rotation.eulerAngles.z < 170) { turnAngle = 60; } else { turnAngle = 300; }
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, turnAngle);
        }

    }
    private void Jets()
    {
        if (Input.GetButton("Jump"))
        {
            Vector3 jetForce = transform.forward * jetSpeed;
            controller.Move((controller.velocity + jetForce * Time.deltaTime) * Time.deltaTime);
            jet.Play();
        }
        else
        {
            jet.Stop();
        }
    }

    private void OnValidate()
    {
        if (flapFraction > 0.4f)
            flapFraction = 0.4f;
        if (flapFraction < 0)
            flapFraction = 0;

        if (stallAngleHigh < 0) stallAngleHigh = 0;
        if (stallAngleLow > 0) stallAngleLow = 0;

        if (chord < 1e-3f)
            chord = 1e-3f;

        if (autoAspectRatio)
            aspectRatio = span / chord;
    }

    // ------------------------- COPY PASTED COEFFICENT CALCULATION CODE ------------------------- //

    private Vector3 CalculateCoefficients(float angleOfAttack,
                                      float correctedLiftSlope,
                                      float zeroLiftAoA,
                                      float stallAngleHigh,
                                      float stallAngleLow)
    {
        Vector3 aerodynamicCoefficients;

        // Low angles of attack mode and stall mode curves are stitched together by a line segment. 
        float paddingAnglHigh = Mathf.Deg2Rad * Mathf.Lerp(15, 5, (Mathf.Rad2Deg * flapAngle + 50) / 100);
        float paddingAnglLow = Mathf.Deg2Rad * Mathf.Lerp(15, 5, (-Mathf.Rad2Deg * flapAngle + 50) / 100);
        float paddedStallAngleHigh = stallAngleHigh + paddingAnglHigh;
        float paddedStallAngleLow = stallAngleLow - paddingAnglLow;

        if (angleOfAttack < stallAngleHigh && angleOfAttack > stallAngleLow)
        {
            // Low angle of attack mode.
            aerodynamicCoefficients = CalculateCoefficientsAtLowAoA(angleOfAttack, correctedLiftSlope, zeroLiftAoA);
        }
        else
        {
            if (angleOfAttack > paddedStallAngleHigh || angleOfAttack < paddedStallAngleLow)
            {
                // Stall mode.
                aerodynamicCoefficients = CalculateCoefficientsAtStall(
                    angleOfAttack, correctedLiftSlope, zeroLiftAoA, stallAngleHigh, stallAngleLow);
            }
            else
            {
                // Linear stitching in-between stall and low angles of attack modes.
                Vector3 aerodynamicCoefficientsLow;
                Vector3 aerodynamicCoefficientsStall;
                float lerpParam;

                if (angleOfAttack > stallAngleHigh)
                {
                    aerodynamicCoefficientsLow = CalculateCoefficientsAtLowAoA(stallAngleHigh, correctedLiftSlope, zeroLiftAoA);
                    aerodynamicCoefficientsStall = CalculateCoefficientsAtStall(
                        paddedStallAngleHigh, correctedLiftSlope, zeroLiftAoA, stallAngleHigh, stallAngleLow);
                    lerpParam = (angleOfAttack - stallAngleHigh) / (paddedStallAngleHigh - stallAngleHigh);
                }
                else
                {
                    aerodynamicCoefficientsLow = CalculateCoefficientsAtLowAoA(stallAngleLow, correctedLiftSlope, zeroLiftAoA);
                    aerodynamicCoefficientsStall = CalculateCoefficientsAtStall(
                        paddedStallAngleLow, correctedLiftSlope, zeroLiftAoA, stallAngleHigh, stallAngleLow);
                    lerpParam = (angleOfAttack - stallAngleLow) / (paddedStallAngleLow - stallAngleLow);
                }
                aerodynamicCoefficients = Vector3.Lerp(aerodynamicCoefficientsLow, aerodynamicCoefficientsStall, lerpParam);
            }
        }
        return aerodynamicCoefficients;
    }

    private Vector3 CalculateCoefficientsAtLowAoA(float angleOfAttack,
                                                  float correctedLiftSlope,
                                                  float zeroLiftAoA)
    {
        float liftCoefficient = correctedLiftSlope * (angleOfAttack - zeroLiftAoA);
        float inducedAngle = liftCoefficient / (Mathf.PI * aspectRatio);
        float effectiveAngle = angleOfAttack - zeroLiftAoA - inducedAngle;

        float tangentialCoefficient = skinFriction * Mathf.Cos(effectiveAngle);

        float normalCoefficient = (liftCoefficient +
            Mathf.Sin(effectiveAngle) * tangentialCoefficient) / Mathf.Cos(effectiveAngle);
        float dragCoefficient = normalCoefficient * Mathf.Sin(effectiveAngle) + tangentialCoefficient * Mathf.Cos(effectiveAngle);
        float torqueCoefficient = -normalCoefficient * TorqCoefficientProportion(effectiveAngle);

        return new Vector3(liftCoefficient, dragCoefficient, torqueCoefficient);
    }

    private Vector3 CalculateCoefficientsAtStall(float angleOfAttack,
                                                 float correctedLiftSlope,
                                                 float zeroLiftAoA,
                                                 float stallAngleHigh,
                                                 float stallAngleLow)
    {
        float liftCoefficientLowAoA;
        if (angleOfAttack > stallAngleHigh)
        {
            liftCoefficientLowAoA = correctedLiftSlope * (stallAngleHigh - zeroLiftAoA);
        }
        else
        {
            liftCoefficientLowAoA = correctedLiftSlope * (stallAngleLow - zeroLiftAoA);
        }
        float inducedAngle = liftCoefficientLowAoA / (Mathf.PI * aspectRatio);

        float lerpParam;
        if (angleOfAttack > stallAngleHigh)
        {
            lerpParam = (Mathf.PI / 2 - Mathf.Clamp(angleOfAttack, -Mathf.PI / 2, Mathf.PI / 2))
                / (Mathf.PI / 2 - stallAngleHigh);
        }
        else
        {
            lerpParam = (-Mathf.PI / 2 - Mathf.Clamp(angleOfAttack, -Mathf.PI / 2, Mathf.PI / 2))
                / (-Mathf.PI / 2 - stallAngleLow);
        }
        inducedAngle = Mathf.Lerp(0, inducedAngle, lerpParam);
        float effectiveAngle = angleOfAttack - zeroLiftAoA - inducedAngle;

        float normalCoefficient = FrictionAt90Degrees() * Mathf.Sin(effectiveAngle) *
            (1 / (0.56f + 0.44f * Mathf.Abs(Mathf.Sin(effectiveAngle))) -
            0.41f * (1 - Mathf.Exp(-17 / aspectRatio)));
        float tangentialCoefficient = 0.5f * skinFriction * Mathf.Cos(effectiveAngle);

        float liftCoefficient = normalCoefficient * Mathf.Cos(effectiveAngle) - tangentialCoefficient * Mathf.Sin(effectiveAngle);
        float dragCoefficient = normalCoefficient * Mathf.Sin(effectiveAngle) + tangentialCoefficient * Mathf.Cos(effectiveAngle);
        float torqueCoefficient = -normalCoefficient * TorqCoefficientProportion(effectiveAngle);

        return new Vector3(liftCoefficient, dragCoefficient, torqueCoefficient);
    }

    private float TorqCoefficientProportion(float effectiveAngle)
    {
        return 0.25f - 0.175f * (1 - 2 * Mathf.Abs(effectiveAngle) / Mathf.PI);
    }

    private float FrictionAt90Degrees()
    {
        return 1.98f - 4.26e-2f * flapAngle * flapAngle + 2.1e-1f * flapAngle;
    }

    private float FlapEffectivnessCorrection(float flapAngle)
    {
        return Mathf.Lerp(0.8f, 0.4f, (flapAngle * Mathf.Rad2Deg - 10) / 50);
    }

    private float LiftCoefficientMaxFraction(float flapFraction)
    {
        return Mathf.Clamp01(1 - 0.5f * (flapFraction - 0.1f) / 0.3f);
    }
}