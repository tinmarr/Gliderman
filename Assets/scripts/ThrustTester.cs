using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrustTester : MonoBehaviour
{
    [Range(0, 1)]
    public float thrustPercent = 0f;
    public PlaneController controller;

    private void Update()
    {
        controller.SetThrust(thrustPercent);
    }
}
