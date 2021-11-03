using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunMovement : MonoBehaviour
{
    public Transform player;
    public float xOffset;
    public float zOffset;


    private void Update()
    {
        transform.position = new Vector3(player.position.x + xOffset, transform.position.y, player.position.z + zOffset);
    }
}
