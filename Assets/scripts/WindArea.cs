using UnityEngine;

public class WindArea : MonoBehaviour
{
    public float Strength;

    void OnTriggerStay(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            AircraftPhysics aircraft = col.GetComponentInParent<AircraftPhysics>();
            aircraft.SetWind(Vector3.up * Strength);
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            AircraftPhysics aircraft = col.GetComponentInParent<AircraftPhysics>();
            aircraft.SetWind(Vector3.zero);
        }
    }
}
