using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunMovement : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = Vector3.zero;

    private void Update()
    {
        transform.position = player.position + offset;
    }
}
