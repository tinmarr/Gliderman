using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public GliderController controller;
    public HotkeyConfig hotkeys;
    [Space(10)]
    public GameObject deadMessage;
    public Text speedDisplay;
    public Text accelerationDisplay;
    public GameObject debugScreen;
    public Image nitroBar;

    float accel = 0;
    float prevV = 0;
    bool f3Screen = false;
    private void Update()
    {
        deadMessage.SetActive(controller.IsDead());
        speedDisplay.text = (int) controller.GetRB().velocity.magnitude + " m/s";
        accelerationDisplay.text = (Mathf.RoundToInt(accel * 10) / 10) + " m/s/s";
        debugScreen.SetActive(f3Screen);

        nitroBar.fillAmount = controller.jetAmount;
        if (controller.jetAmount < 0.2f) nitroBar.color = Color.red;
        else if (controller.jetAmount < 0.5f) nitroBar.color = new Color(1, 0.5f, 0, 1);
        else if (controller.jetAmount <= 1f) nitroBar.color = Color.yellow;

        if (Input.GetKeyDown(hotkeys.debugMode))
        {
            f3Screen = !f3Screen;
        }
    }

    private void FixedUpdate()
    {
        accel = (controller.GetRB().velocity.magnitude - prevV) / Time.fixedDeltaTime;
        prevV = controller.GetRB().velocity.magnitude;
    }
}
