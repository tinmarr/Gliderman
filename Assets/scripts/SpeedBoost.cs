using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    public int time = 2;
    public float thrustPercent = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlaneController contoller = other.gameObject.GetComponentInParent<PlaneController>();
            contoller.SetThrust(thrustPercent, time);
        }
    }
}
