using UnityEngine;

public class WindArea : MonoBehaviour
{
    public float Strength;
    public Vector3 WindDirection;

    void OnTriggerStay(Collider col)
    {
        if (col.tag == "Player")
        {
            Glider2 script = col.GetComponent<Glider2>();
            script.AddForce(new Vector3(0, 1000, 0), false, true);
        }
    }
}
