using UnityEngine;

public enum Side { Right, Left}

public class FlapController : MonoBehaviour
{
    public float flapAngle = 0;
    public Side side;
    private Vector3 initialRotation;

    float multiplier = 10;
    public float smoothSpeed = 0.1f;

    /// <summary>
    /// Set Flap Angle
    /// </summary>
    /// <param name="flapAngle">Number between -70 and 70</param>
    public void SetFlap(float flapAngle)
    {
        this.flapAngle = Mathf.Clamp(flapAngle, -70, 70);
    }

    private void Start()
    {
        initialRotation = transform.localRotation.eulerAngles;

        if (side == Side.Right)
        {
            multiplier *= -1f;
        }
    }

    private void Update()
    {
        float goalx = initialRotation.x + flapAngle;
        float goaly = initialRotation.y + flapAngle/multiplier;
        transform.localRotation = Quaternion.Euler(goalx, goaly, initialRotation.z);
    }
}
