using UnityEngine;

public enum Side { Right, Left}

public class FlapController : MonoBehaviour
{
    public float flapAngle = 0;
    public Side side;
    private Vector3 initialRotation;

    private float multiplier = 10;


    public void SetFlap(float flapAngle)
    {
        this.flapAngle = flapAngle;
    }

    public void Start()
    {
        initialRotation = transform.localRotation.eulerAngles;

        if (side == Side.Right)
        {
            multiplier *= -1f;
        }
    }

    public void Update()
    {
        float goalx = initialRotation.x + flapAngle;
        float goaly = initialRotation.y + flapAngle/multiplier;
        transform.localRotation = Quaternion.Euler(goalx, goaly, initialRotation.z);
    }
}
