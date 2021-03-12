using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            PlaneController contoller = other.gameObject.GetComponentInParent<PlaneController>();
            contoller.setThrust(1f, 2);
        }
    }
}
