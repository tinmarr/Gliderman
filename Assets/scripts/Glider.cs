using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glider : MonoBehaviour
{
    Rigidbody rigidbody;
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
        rigidbody = gameObject.GetComponent<Rigidbody>();
        particle.Stop();
    }
    // Update is called once per frame
    void Update()
    {
        energy = transform.position.y + rigidbody.velocity.magnitude;
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

        // yaw cant really be adjusted by player
        // yaw is if we had a string from top to bottom adjustment
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
        if ((transform.forward + rigidbody.velocity.normalized).magnitude < 1.4) tilt += .3f;

        //rotate yourself around the anchor point of your side down and up
        if (tilt != 0)
            transform.Rotate(transform.right, tilt * Time.deltaTime * 10, Space.World);
        //rotate yourself around the anchor point of your forward to the right and left
        if (roll != 0)
            transform.Rotate(transform.forward, roll * Time.deltaTime * -10, Space.World);
        else { 
            if (359 > transform.eulerAngles.z || transform.eulerAngles.z > 1){
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

    }
    private void Jets()
    {
        if (Input.GetButton("Jump"))
        {
            // find how much this force is equivalent to.
            rigidbody.AddForce(transform.forward * jetPower);
            particle.Play();
        }
        else
        {
            //maybe do something else
            //particle.Stop();
        }
        //if (Input.GetButtonDown("Jump")) UseBlasters();
    }
    private void Gravity() 
    {
        //GRAVITY
        rigidbody.velocity -= gravity * Vector3.up * Time.deltaTime;
    }
    void DragAndOthers() 
    {

        // Vertical (to the glider) velocity truns into horizontal velocity
        // getting just the up velocity relevant to our transform
        Vector3 vertvel = rigidbody.velocity - Vector3.Exclude(transform.up, rigidbody.velocity);
        rigidbody.velocity -= vertvel * Time.deltaTime; // take out the velocity that was pushing us "up"
        rigidbody.velocity += vertvel.magnitude * transform.forward * Time.deltaTime / 5; // and add it going forward


        // Drag
        // get our forward velocity relative to glider
        Vector3 forwardDrag = rigidbody.velocity - Vector3.Exclude(transform.forward, rigidbody.velocity);
        //add drag in opposite direction from forward drag
        rigidbody.AddForce(-forwardDrag * forwardDrag.magnitude * Time.deltaTime / 1000);

        Vector3 sideDrag = rigidbody.velocity - Vector3.Exclude(transform.right, rigidbody.velocity);
        // much more aggressive side drag as gliders move very badly sideways
        rigidbody.AddForce(-sideDrag * sideDrag.magnitude * Time.deltaTime);
        rigidbody.velocity += transform.forward * sideDrag.magnitude * Time.deltaTime / 10;
    }
    void TerminalVelocity() 
    {
        Vector3.ClampMagnitude(rigidbody.velocity, terminalVelocity);
    }
    private void OnDrawGizmos()
    {
        if (rigidbody) Debug.DrawLine(transform.position, transform.position + rigidbody.velocity, Color.green);
        Debug.DrawLine(transform.position, transform.position + Vector3.up * 2, Color.red);
    }

    public void UseBlasters()
    {
        StartCoroutine("blast");
    }
    public IEnumerator blast()
    {
        rigidbody.AddForce(transform.forward * jetPower);
        particle.Play();
        yield return new WaitForSeconds(3);
        particle.Stop();
    }
}
