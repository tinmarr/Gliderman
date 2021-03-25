using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public GliderController controller;
    public HotkeyConfig hotkeys;
    [Header("Main")]
    public GameObject deadMessage;
    public Text speedDisplay;
    public Text accelerationDisplay;
    public Image nitroBar;
    public RectTransform plane;
    public Transform plane2;
    public Transform gliderBig;
    public Text pitch;
    public Text roll;
    public Text timer;
    [Header("Debug")]
    public GameObject debugScreen;
    public float refreshTime = 0.5f;
    public Text leftSide;

    float accel = 0;
    float prevV = 0;
    bool f3Screen = false;

    int frameCounter = 0;
    float timeCounter = 0.0f;
    float fps = 0.0f;

    int frozenTime = -1;
    private void Update()
    {
        int timeToDisplay = 0;
        if (controller.IsDead() && frozenTime == -1)
        {
            frozenTime = (int)(Time.realtimeSinceStartup - controller.GetAliveSince());
        } else if (controller.IsDead())
        {
            timeToDisplay = frozenTime;
        } else if (controller.GetLaunched())
        {
            timeToDisplay = (int)(Time.realtimeSinceStartup - controller.GetAliveSince());
            frozenTime = -1;
        }
        timer.text = $"Time Alive:\n{timeToDisplay}s";

        deadMessage.SetActive(controller.IsDead());
        speedDisplay.text = (int) controller.GetRB().velocity.magnitude + " m/s";
        accelerationDisplay.text = (Mathf.RoundToInt(accel * 10) / 10) + " m/s/s";
        debugScreen.SetActive(f3Screen);

        Vector3 angles = controller.transform.localEulerAngles;
        if (angles.x > 180) angles.x -= 360;
        if (angles.z > 180) angles.z -= 360;
        pitch.text = (int)-angles.x + "�";
        roll.text = (int)-angles.z + "�";
        angles.x += 90;
        //plane.rotation = Quaternion.Euler(angles.x, 0, angles.z);

        plane2.localRotation = Quaternion.Euler(0, 180, angles.z);
        Transform modelBit = plane2.GetComponentInChildren<Transform>();
        modelBit.localRotation = Quaternion.Euler(angles.x, -90, 90);

        nitroBar.fillAmount = controller.jetAmount;
        if (controller.jetAmount < 0.2f) nitroBar.color = new Color32(0xe7, 0x4c, 0x3c, 0xff);
        else if (controller.jetAmount < 0.5f) nitroBar.color = new Color32(0xf3, 0x9c, 0x12, 0xff);
        else if (controller.jetAmount <= 1f) nitroBar.color = new Color32(0x27, 0xae, 0x60, 0xff);
        
        if (Input.GetKeyDown(hotkeys.debugMode))
        {
            f3Screen = !f3Screen;
        }

        if (timeCounter < refreshTime)
        {
            timeCounter += Time.smoothDeltaTime;
            frameCounter++;
        }
        else
        {
            fps = (float)frameCounter / timeCounter;
            frameCounter = 0;
            timeCounter = 0.0f;
        }

        leftSide.text = @$" FPS: {(int)fps}
 Position: {controller.transform.position.x} / {controller.transform.position.y} / {controller.transform.position.z}
 Distance to Nearest: {controller.GetMinDistance()}
";
    }

    private void FixedUpdate()
    {
        accel = (controller.GetRB().velocity.magnitude - prevV) / Time.fixedDeltaTime;
        prevV = controller.GetRB().velocity.magnitude;
    }
}