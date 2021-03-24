using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Plane Balancing Config", menuName = "Plane Balancing Config")]
public class PlaneBalanceConfig : ScriptableObject
{
    public bool liveUpdate = false;
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

    [Header("Nitro")]
    public AnimationCurve proximityCurve;
    [Tooltip("How much much time a full nitrobar lasts (sec)")]
    public float decreaseTime = 200;
    [Tooltip("How much much time it takes to fill up a full nitrobar when going 100m/s (sec)")]
    public float increaseTime = 100;
    [Tooltip("How much velocity impacts nitro fill up speed (Bigger numbers = less impact)")]
    public float impactOfVelocity = 5;
    [Tooltip("How much power thrust has")]
    public float thrust = 0;

    [Header("Other")]
    [Tooltip("Min velocity needed to brake")]
    public float minVelocity = 30;
    [Tooltip("Velocity at which most lerp functions stop and where boosting stops")]
    public float terminalVelocity = 200;
}
