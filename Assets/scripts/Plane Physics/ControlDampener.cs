using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlDampener : MonoBehaviour
{
    GliderController controller;

    public AnimationCurve pitchCurve;
    public AnimationCurve rollCurve;

    [Range(-1,1)]
    public float pitchChange;
    [Range(-1,1)]
    public float yawChange;

    private void Start()
    {
        controller = GetComponentInParent<GliderController>();
    }

    public void Dampen(ref float pitch, ref float roll, float velocity, float terminalVelocity)
    {
        // find way to access terminal velocity instead of 200
        pitch *= this.pitchCurve.Evaluate(Mathf.InverseLerp(0, terminalVelocity, velocity));
        roll *= this.rollCurve.Evaluate(Mathf.InverseLerp(0, terminalVelocity, velocity));
        // pitch between -1 and 1, the higher the velocity the same it will be
    }
}