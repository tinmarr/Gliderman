using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlDampener : MonoBehaviour
{

    public AnimationCurve pitchCurve;
    public AnimationCurve rollCurve;

    public void DampenPitch(ref float pitch, ref float roll, float velocity, float terminalVelocity)
    {
        // find way to access terminal velocity instead of 200
        pitch *= this.pitchCurve.Evaluate(Mathf.InverseLerp(0, terminalVelocity, velocity));
        roll *= this.rollCurve.Evaluate(Mathf.InverseLerp(0, terminalVelocity, velocity));
        // pitch between -1 and 1, the higher the velocity the same it will be
    }
}
