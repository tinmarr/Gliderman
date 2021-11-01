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
    public float rollControlSensitivity = 0.2f;
    [Tooltip("Basic multiplier for roll")]
    public float pitchControlSensitivity = 0.2f;
    [Tooltip("Basic multiplier for roll")]
    public float yawControlSensitivity = 0.2f;
    [Tooltip("Curve that maps speed to a pitch multipler")]
    public AnimationCurve pitchCurve;
    [Tooltip("Curve that maps speed to a roll multipler")]
    public AnimationCurve rollCurve;
    [Tooltip("Curve that maps nitro increase to distance from ground")]

    [Header("Flaps")]
    [Range(0, 1)]
    public float flapOpenSpeed = 0.08f;
    [Range(0, 1)]
    public float closeSpeed = 0.2f;

    [Header("Jet Parameters")]
    public float thrust = 1800;
    public AnimationCurve proximityCurve;
    public float decreaseTime = 200;
    public float increaseTime = 100;
    [Tooltip("Bigger values shrink the impact of velocity (increaseMultiplier = velocity/impactOfVelocity)")]
    public float impactOfVelocity = 5;

    [Header("Physics")]
    public float terminalVelocity = 200f;
    public int physicsSubsteps = 2;
}
