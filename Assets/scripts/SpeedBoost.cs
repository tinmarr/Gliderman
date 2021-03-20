using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    public int time = 2;
    public float thrustPercent = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GliderController contoller = other.gameObject.GetComponentInParent<GliderController>();
            contoller.SetThrust(thrustPercent, time);
        }
    }
}
