using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    public int time = 2;
    public float thrustPercent = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GliderController controller = other.gameObject.GetComponentInParent<GliderController>();
            controller.SetThrust(thrustPercent, time);
            controller.jetAmount = 1f;
        }
    }
}
