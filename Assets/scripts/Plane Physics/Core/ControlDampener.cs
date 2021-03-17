using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlDampener : MonoBehaviour
{

    public AnimationCurve pitchCurve;
    public AnimationCurve rollCurve;

    [Range(-1,1)]
    public float pitchChange;
    [Range(-1,1)]
    public float yawChange;

    public void Dampen(ref float pitch, ref float roll, float velocity, float terminalVelocity)
    {
        // find way to access terminal velocity instead of 200
        pitch *= this.pitchCurve.Evaluate(Mathf.InverseLerp(0, terminalVelocity, velocity));
        roll *= this.rollCurve.Evaluate(Mathf.InverseLerp(0, terminalVelocity, velocity));
        // pitch between -1 and 1, the higher the velocity the same it will be
    }

    public void TurnSmooth(ref float pitch,  float roll, ref float yaw)
    {
        pitch += pitchChange;
        yaw += roll / Mathf.Abs(roll) * yawChange;
        // basic idea:
        // pitch slightly up
        // yaw slightly to place you turn
        // possibly
    }

}