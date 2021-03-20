using UnityEngine;

public class FlapController : MonoBehaviour
{
    public float flapAngle = 0;
    private Vector3 initialRotation;
    /// <summary>
    /// Set Flap Angle
    /// </summary>
    /// <param name="flapAngle">Number between -90 and 90</param>
    public void SetFlap(float flapAngle)
    {
        this.flapAngle = Mathf.Clamp(flapAngle, -90, 90);
    }

    private void Start()
    {
        initialRotation = transform.localRotation.eulerAngles;
    }

    private void Update()
    {
        transform.localRotation = Quaternion.Euler(
            initialRotation.x + flapAngle, 
            initialRotation.y, 
            initialRotation.z
           );
    }
}
