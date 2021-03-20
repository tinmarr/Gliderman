using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Automation : MonoBehaviour
{
    GliderController controller;

    [Header("Noob Settings")]
    [SerializeField] bool angleClamp = true;
    [SerializeField] bool autoTurn = true;
    [SerializeField] bool autoCorrect = true;
    
    bool turnStart = false;
    float turnAltitude = 0;

    private void Start()
    {
        controller = GetComponent<GliderController>();
    }

    public void NoobSettings(ref float Pitch, ref float Yaw, ref float Roll)
    {
        if (autoTurn)
        {
            AutoTurner(ref Pitch, ref Yaw, ref Roll);
        }
        if (angleClamp)
        {
            AngleClamp();
        }
        if (autoCorrect)
        {
            AutoCorrecter(ref Roll);
        }
        
    }

    private void AutoTurner(ref float Pitch, ref float Yaw, ref float Roll)
    {
        if (Roll != 0 && Pitch == 0)
        {
            if (!turnStart)
            {
                turnAltitude = transform.position.y;
                turnStart = true;
            }
            TurnSmooth(ref Pitch, Roll, ref Yaw, turnAltitude);
        }
        else { turnStart = false; }
    }

    private void AutoCorrecter(ref float Roll)
    {
        if (Roll == 0)
        {
            if (359 > transform.eulerAngles.z && transform.eulerAngles.z > 1)
            {
                Roll = (transform.eulerAngles.z < 60) ? 1 : -1;
            }
        }
    }
    private void AngleClamp()
    {
        Vector3 angle = transform.eulerAngles;
        if (angle.x > 180) angle.x -= 360;
        if (angle.y > 180) angle.y -= 360;
        if (angle.z > 180) angle.z -= 360;

        transform.rotation = Quaternion.Euler(
            Mathf.Clamp(angle.x, -80, 80),
            angle.y,
            Mathf.Clamp(angle.z, -60, 60)
        );
    }

    public void TurnSmooth(ref float pitch, float roll, ref float yaw, float altitude)
    {
        //if (pitch < 0) pitch += 1 / 3 * pitchChange; else pitch += pitchChange;
        //yaw += roll / Mathf.Abs(roll) * yawChange;

        pitch = (controller.transform.position.y - altitude) / 20;

        // basic idea:
        // pitch slightly up
        // yaw slightly to place you turn
        // possibly
    }
}
