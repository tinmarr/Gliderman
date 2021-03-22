using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPad : MonoBehaviour
{
    public Rigidbody player;
    public float startStrengthUp;
    public float startStrengthForward;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKey(KeyCode.L))
        {
            player.AddForce((player.transform.forward * startStrengthForward) + (player.transform.up * startStrengthUp));
        }
    }

    private void OnValidate()
    {
        startStrengthUp = Mathf.Clamp(startStrengthUp, 0, Mathf.Infinity);
        startStrengthForward = Mathf.Clamp(startStrengthForward, 0, Mathf.Infinity);
    }
}
