using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlDampener : MonoBehaviour
{

    public AnimationCurve pitchCurve;

    public void DampenPitch(ref float pitch, float velocity)
    {
        // find way to access terminal velocity instead of 200
        pitch = pitch * this.pitchCurve.Evaluate(Mathf.InverseLerp(0, 200, velocity));
        // pitch between -1 and 1, the higher the velocity the same it will be
    }
}
