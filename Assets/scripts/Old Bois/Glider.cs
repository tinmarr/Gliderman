using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glider : MonoBehaviour
{
    Rigidbody rb;
    public float gravity = 9.8f;
    public float terminalVelocity = 40;
    public int jetPower = 900;
    public float rollSpeed = 2;
    public float tiltSpeed = 2;
    public ParticleSystem particle;
    public float anglefromground;
    public float energy;

    
    float roll ;
    float tilt ;
    float yaw ;
    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        particle.Stop();
    }
    // Update is called once per frame
    void Update()
    {
        energy = transform.position.y + rb.velocity.magnitude;
        //if (Input.GetButtonDown("Fire1"))
        //{
        //    Debug.Log("here");
        //    // find how much this force is equivalent to.
        //    Instantiate(particle, transform, true);
        //    particle.Play();
        //    particle.transform.position += transform.forward;
        //}
        anglefromground = Vector3.Dot(Vector3.forward, transform.forward);
    }
    private void FixedUpdate()
    {
        GetInputs();
        AdjustRotations();
        if (Input.GetKeyDown(KeyCode.Space))
            Jets();
        Gravity();
        DragAndOthers();
        TerminalVelocity();

    }
    private void GetInputs()
    {
        // turn body from forward to backward
        roll = Input.GetAxis("Horizontal") * rollSpeed / Time.timeScale;
        // go up or down
        tilt = Input.GetAxis("Vertical") * tiltSpeed / Time.timeScale;

        // roll cant really be adjusted by player
        // roll is if we had a string from top to bottom adjustment
        yaw = 0;

        // if your world.up is | and transform.right is -- then the we havent turned our body
        // and the magnitude of that vector would be exactly 1.414214
        // the dot product is shorter if the angles are farther apart
        float tip = Vector3.Dot(transform.right, Vector3.up);// or we could just get the dot product
        //float tip = (transform.right + Vector3.up).magnitude - 1.414214f;
        yaw -= tip;
        // float tip = Vector3.Dot(transform.right, Vector3.up); <-- find out about that

    }
    private void AdjustRotations()
    {
        //stall prevention <-- find out
        if ((transform.forward + rb.velocity.normalized).magnitude < 1.4) tilt += .3f;

        //rotate yourself around the anchor point of your side down and up
        if (tilt != 0)
            transform.Rotate(transform.right, tilt * Time.deltaTime * 10, Space.World);
        //rotate yourself around the anchor point of your forward to the right and left
        if (roll != 0)
            transform.Rotate(transform.forward, roll * Time.deltaTime * -10, Space.World);
        else { 
            if (359 > transform.eulerAngles.z && transform.eulerAngles.z > 1){
                roll = (transform.eulerAngles.z < 60) ? 1 : -1;
                transform.Rotate(transform.forward, roll * Time.deltaTime * -10, Space.World);
            }
        }
            
        //this one is iffy
        if (yaw != 0)
            transform.Rotate(Vector3.up, yaw * Time.deltaTime * 15, Space.World);

        if (transform.rotation.eulerAngles.z > 60 && transform.rotation.eulerAngles.z < 300)
        {
            int turnAngle;
            if (transform.rotation.eulerAngles.z < 170) { turnAngle = 60; } else { turnAngle = 300; }
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, turnAngle);
        }
        if (transform.rotation.eulerAngles.x > 70 && transform.rotation.eulerAngles.x < 290)
        {
            int turnAngle;
            if (transform.rotation.eulerAngles.x < 170) { turnAngle = 60; } else { turnAngle = 300; }
            transform.eulerAngles = new Vector3(turnAngle, transform.eulerAngles.y, transform.eulerAngles.z);
        }

    }
    private void Jets()
    {
        UseBlasters();
    }
    private void Gravity() 
    {
        //GRAVITY
        rb.velocity -= gravity * Vector3.up * Time.deltaTime;
    }
    void DragAndOthers() 
    {

        // Vertical (to the glider) velocity truns into horizontal velocity
        // getting just the up velocity relevant to our transform
        Vector3 vertvel = rb.velocity - Vector3.ProjectOnPlane(transform.up, rb.velocity);
        rb.velocity -= vertvel * Time.deltaTime; // take out the velocity that was pushing us "up"
        rb.velocity += vertvel.magnitude * transform.forward * Time.deltaTime / 5; // and add it going forward


        // Drag
        // get our forward velocity relative to glider
        Vector3 forwardDrag = rb.velocity - Vector3.ProjectOnPlane(transform.forward, rb.velocity);
        //add drag in opposite direction from forward drag
        rb.AddForce(-forwardDrag * forwardDrag.magnitude * Time.deltaTime / 1000);

        Vector3 sideDrag = rb.velocity - Vector3.ProjectOnPlane(transform.right, rb.velocity);
        // much more aggressive side drag as gliders move very badly sideways
        rb.AddForce(-sideDrag * sideDrag.magnitude * Time.deltaTime);
        rb.velocity += transform.forward * sideDrag.magnitude * Time.deltaTime / 10;
    }
    void TerminalVelocity() 
    {
        Vector3.ClampMagnitude(rb.velocity, terminalVelocity);
    }
    private void OnDrawGizmos()
    {
        if (rb) Debug.DrawLine(transform.position, transform.position + rb.velocity, Color.green);
        Debug.DrawLine(transform.position, transform.position + Vector3.up * 2, Color.red);
    }

    public void UseBlasters()
    {
        Debug.Log("here");
        StartCoroutine("blast");
    }
    public IEnumerator blast()
    {
        particle.Play();
        for (float jet = 1f; jet >= 0; jet -= 0.1f) //possible implementation of jet time
        {
            rb.AddForce(transform.forward * jetPower);
            yield return new WaitForSeconds(.1f);
        }
        particle.Stop();
    }
}
