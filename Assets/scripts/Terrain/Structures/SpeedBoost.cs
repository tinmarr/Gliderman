using UnityEngine;
using System.Collections;

public class SpeedBoost : MonoBehaviour
{
    public float time = 2f;
    public float thrustPercent = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GliderController controller = other.gameObject.GetComponentInParent<GliderController>();
            controller.SetThrust(thrustPercent);
            StartCoroutine(KillThrust(controller, time));
        }
    }

    private IEnumerator KillThrust(GliderController controller, float time)
    {
        yield return new WaitForSeconds(time);
        controller.SetThrust(0);
    }
}
