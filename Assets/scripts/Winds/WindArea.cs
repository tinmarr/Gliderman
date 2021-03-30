using UnityEngine;

public class WindArea : MonoBehaviour
{
    public float Strength;
    public AnimationCurve curveSteepness;
    public SoundManager soundManager;

    void OnTriggerStay(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            AircraftPhysics aircraft = col.GetComponentInParent<AircraftPhysics>();
            Vector3 fakeTransform = transform.position;
            fakeTransform.y = 0;
            Vector3 fakeGlider = col.transform.position;
            fakeGlider.y = 0;
            float distance = Vector3.Distance(fakeTransform, fakeGlider);
            float maxDistanceInside = transform.localScale.x/2;    
            float coefficient = curveSteepness.Evaluate(distance / maxDistanceInside);
            Vector3 wind = Vector3.up * Strength * coefficient;
            aircraft.SetWind(wind);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            soundManager.Play("startMusic", 0.1f);
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            soundManager.StopSound("startMusic");
            AircraftPhysics aircraft = col.GetComponentInParent<AircraftPhysics>();
            aircraft.SetWind(Vector3.zero);
        }
    }

}
