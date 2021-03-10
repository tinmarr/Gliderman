using UnityEngine;

public class WindArea : MonoBehaviour
{
    public float Strength;
    public Vector3 WindDirection;

    void OnTriggerStay(Collider col)
    {
        if (col.tag == "Player")
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            rb.AddForce(new Vector3(0, 1000, 0));
        }
    }
}
