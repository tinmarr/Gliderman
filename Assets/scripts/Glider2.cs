using UnityEngine;

public class Glider2 : MonoBehaviour
{
    public float gravity = -9.8f;
    public float liftMultiplier = 1f;
    
    public float mass = 10f;
    public float wingArea = 20f;

    public float areaDensity = 1.225f;
    public float coefficentOfDrag = 1f;

    public ParticleSystem jet;

    private CharacterController controller;
    private Vector3 acceleration = new Vector3(0, 0, 0);
    

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        jet.Stop();
    }

    private void Update()
    {
        ApplyPhysics();
    }

    void ApplyPhysics()
    {
        Vector3 gravityForce = new Vector3(0, mass * gravity, 0);

        // Vector3 dragForce = transform.up * coefficentOfDrag * (areaDensity * (Mathf.Pow(controller.velocity.x,2) + Mathf.Pow(controller.velocity.z, 2))) / 2;
        // Vector3 dragForce = Mathf.Pow(controller.velocity, 2);
        Vector3 dragForce = transform.up * coefficentOfDrag * (areaDensity * (Mathf.Pow(controller.velocity.y, 2)) * wingArea) / 2;


        Vector3 netAccel = (gravityForce + dragForce) / mass;
        Vector3 newVelocity = controller.velocity + netAccel * Time.deltaTime;
        controller.Move(newVelocity * Time.deltaTime);
    }
}
