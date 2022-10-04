using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Glider Config", menuName = "Glider Controller Config")]
public class GliderControllerConfig : ScriptableObject
{
    [Header("Debug")]
    public bool fuelHack = false;

    [Header("Control")]
    [Tooltip("Basic multiplier for roll")]
    public float rollControlSensitivity = 0.1f;
    [Tooltip("Basic multiplier for roll")]
    public float pitchControlSensitivity = 0.1f;
    [Tooltip("Basic multiplier for roll")]
    public float yawControlSensitivity = 0.2f;
    [Tooltip("Curve that maps speed to a pitch multipler")]
    public AnimationCurve pitchCurve = new AnimationCurve();
    [Tooltip("Curve that maps speed to a roll multipler")]
    public AnimationCurve rollCurve = new AnimationCurve();
    [Tooltip("Curve that maps speed to a yaw multipler")]
    public AnimationCurve yawCurve = new AnimationCurve();
    

    [Header("Flaps")]
    [Range(0, 1)]
    public float flapOpenSpeed = 0.08f;
    [Range(0, 1)]
    public float closeSpeed = 0.2f;

    [Header("Jet Parameters")]
    public float thrust = 1800;
    [Tooltip("Curve that maps nitro increase to distance from ground")]
    public AnimationCurve proximityCurve = new AnimationCurve();
    public float decreaseTime = 4;
    public float increaseTime = 5;
    [Tooltip("Bigger values shrink the impact of velocity (increaseMultiplier = velocity/impactOfVelocity)")]
    public float impactOfVelocity = 4;

    [Header("Physics")]
    public float terminalVelocity = 350;
    public float dragAmount = 0.001f;
    [Range(0, 500)]
    public float groundEffectThreshold = 15f;

    public GliderControllerConfig clone()
    {
        return Instantiate(this);
    }
}
