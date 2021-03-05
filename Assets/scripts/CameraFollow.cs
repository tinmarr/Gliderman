using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    [Range(0,1)]
    public float smoothSpeed = .5f;
    public Vector3 offset;

    private void FixedUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothPosition;
        Quaternion smoothRotation = Quaternion.Lerp(transform.rotation, target.rotation, smoothSpeed);
        transform.rotation = smoothRotation;
        transform.LookAt(target);
    }
}
