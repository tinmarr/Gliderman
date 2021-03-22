using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPad : MonoBehaviour
{
    public GliderController player;
    public float startStrengthUp;
    public float startStrengthForward;
    public float throttling = 0.1f;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.L))
        {
            Vector3 finalForce = (player.transform.forward * startStrengthForward) + (player.transform.up * startStrengthUp);
            Vector3 currentForce = Vector3.zero;
            for (int i = 0; i < 1/throttling; i++)
            {
                currentForce = Vector3.Lerp(currentForce, finalForce, throttling);
                StartCoroutine(Launch(currentForce, throttling * i));
            }
        }
    }

    public IEnumerator Launch(Vector3 thrust, float delay)
    {
        yield return new WaitForSeconds(delay);
        player.GetRB().AddForce(thrust);
        player.jetAmount = 0f;
    }

    private void OnValidate()
    {
        startStrengthUp = Mathf.Clamp(startStrengthUp, 0, Mathf.Infinity);
        startStrengthForward = Mathf.Clamp(startStrengthForward, 0, Mathf.Infinity);
    }
}
