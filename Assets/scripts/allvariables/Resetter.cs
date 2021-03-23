using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resetter : MonoBehaviour
{
    // all stuff that need resetting
    // most commented stuff are just there because they had errors need to look over that.
    SpeedBoost speedBoost;
    StartPad startPad;
    WindArea windArea;
    FlapController flapController;
    ControlDampener controlDampener;
    Automation automation;
    GliderController gliderController;
    private void Start()
    {
        speedBoost = gameObject.GetComponent<SpeedBoost>();
        startPad = gameObject.GetComponent<StartPad>();
        windArea = gameObject.GetComponent<WindArea>();
        flapController = gameObject.GetComponent<FlapController>();
        controlDampener = gameObject.GetComponent<ControlDampener>();
        automation = gameObject.GetComponent<Automation>();
        gliderController = gameObject.GetComponent<GliderController>();
    }
    public class SpeedBoost : MonoBehaviour
    {
        public int time = 2;
        public float thrustPercent = 1f;
    }
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
                for (int i = 0; i < 1 / throttling; i++)
                {
                    currentForce = Vector3.Lerp(currentForce, finalForce, throttling);
                    StartCoroutine(Launch(currentForce, throttling * i));
                }
            }
        }

        public IEnumerator Launch(Vector3 thrust, float delay)
        {
            yield return new WaitForSeconds(delay);
            /*player.GetRB().AddForce(thrust);*/
            player.jetAmount = 0f;
        }

        private void OnValidate()
        {
            startStrengthUp = Mathf.Clamp(startStrengthUp, 0, Mathf.Infinity);
            startStrengthForward = Mathf.Clamp(startStrengthForward, 0, Mathf.Infinity);
        }
    }
    public class WindArea : MonoBehaviour
    {
        public float Strength;
        public int timeUntilStrength = 5;
        public int curveSteepness = 4;
        public float timeInWind = 0;
    }
    public class FlapController : MonoBehaviour
    {
        public float flapAngle = 0;
        public Side side;
        private Vector3 initialRotation;

        float multiplier = 10;
        public float smoothSpeed = 0.1f;
    }
    public class ControlDampener : MonoBehaviour
    {
        GliderController controller;

        public AnimationCurve pitchCurve;
        public AnimationCurve rollCurve;

        [Range(-1, 1)]
        public float pitchChange;
        [Range(-1, 1)]
        public float yawChange;
    }
    public class Automation : MonoBehaviour
    {
        [Header("Noob Settings")]
        public bool angleClamp = true;
        public bool autoTurn = true;
        public bool autoCorrect = true;

        // do these need to be reset, I believe so, how we need setter functions
        bool turnStart = false;
        float turnAltitude = 0;
    }
    public class GliderController : MonoBehaviour
    {
        [Header("Control Parameters")]
        [SerializeField]
        List<AeroSurface> controlSurfaces = null;
        [SerializeField]
        float rollControlSensitivity = 0.2f;
        [SerializeField]
        float pitchControlSensitivity = 0.2f;
        [SerializeField]
        float yawControlSensitivity = 0.2f;
        float[] sensitivitySaves = new float[3];
        public FlapController[] flaps;
        float[] flapAngles = { 0, 0, 0, 0 }; // top to bottom then left to right
        [Range(0, 1)]
        public float smoothSpeed = 0.08f;
        [Range(0, 1)]
        public float closeSpeed = 0.2f;

        [Header("Display Variables")]
        [Range(-1, 1)]
        public float Pitch;
        [Range(-1, 1)]
        public float Yaw;
        [Range(-1, 1)]
        public float Roll;
        [Range(-1, 1)]
        public float Flap;
        [Tooltip("Toggled by shift, this helps you do the stuffs")]
        public bool noobSettings;

        [Header("Jet Parameters")]
        public float thrustPercent;
        AircraftPhysics aircraftPhysics;
        Rigidbody rb;
        public ParticleSystem jet;
        public AnimationCurve proximityCurve;
        bool speeding = false;
        bool jetEmpty = false;
        public float jetAmount = 0f;
        public float decreasePerSecondSpeed = 200;
        public float increasePerSecondSpeed = 100;

        [Header("Trails")]
        public TrailRenderer rightTrail;
        public TrailRenderer leftTrail;
        public Material trailNormal;
        public Material trailBoost;
        public Material trailGround;

        [Header("Dampening Parameters")]
        // dampening
        public float terminalVelocity = 200f;
        public ControlDampener controlDampener;

        [Header("UI")]
        /*public Text planeInfo;*/
        public GameObject ded;

        [Header("Camera")]
        /*
        public CinemachineVirtualCamera followCam;
        public CinemachineVirtualCamera followCamRoll;
        public CinemachineVirtualCamera shakeCam;
        public CinemachineVirtualCamera shakeCamRoll;
        CinemachineVirtualCamera currentCam;
        */
        [Header("Brakes")]
        public Transform[] brakes = new Transform[2];
        public float minVelocity = 30;

        [Header("Other")]
        bool dead = false;
        Vector3 startPos;
        Quaternion startRot;
        Vector3 startScale;
        Automation automation;
    }
}