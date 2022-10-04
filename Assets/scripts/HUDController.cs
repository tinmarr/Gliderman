using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class HUDController : MonoBehaviour
{
    public GliderController controller;
    [Header("Main")]
    public Text speedDisplay;
    public Text accelerationDisplay;
    public Image nitroBar;
    public RectTransform plane;
    public Text pitch;
    public Text roll;
    public TMP_Text infoMessage;

    [Header("Debug")]
    public GameObject debugScreen;
    public float refreshTime = 0.5f;
    public Text debugLeft;
    public Text debugRight;

    [Header("Curves")]
    public AnimationCurve nitroBarCurve;

    Queue<string> messages = new Queue<string>();

    bool f3Screen = false;
    public bool isWanted = true;

    int frameCounter = 0;
    float timeCounter = 0.0f;
    float fps = 0.0f;

    private void Start()
    {
        controller.input.actions["Debug"].performed += _ => { f3Screen = !f3Screen; };
        Update();
        infoMessage.gameObject.SetActive(false);
    }

    private void Update()
    {
        speedDisplay.text = (int) controller.rb.velocity.magnitude + " m/s";
        accelerationDisplay.text = controller.currentScore + " pts";
        debugScreen.SetActive(f3Screen);

        Vector3 angles = controller.transform.localEulerAngles;
        if (angles.x > 180) angles.x -= 360;
        if (angles.z > 180) angles.z -= 360;
        pitch.text = (int)-angles.x + "°";
        roll.text = (int) angles.z + "°";
        angles.x += 90;
        plane.rotation = Quaternion.Euler(angles.x, 0, angles.z);

        nitroBar.fillAmount = controller.fuelAmount;
        Color32 redColor = new Color32(0xe7, 0x4c, 0x3c, 0xff);
        Color32 yellowColor = new Color32(0xf3, 0x9c, 0x12, 0xff);
        Color32 greenColor = new Color32(0x27, 0xae, 0x60, 0xff);
        if (controller.fuelAmount < 0.2f) nitroBar.color = Color32.Lerp(redColor, yellowColor, nitroBarCurve.Evaluate(controller.fuelAmount * 5));
        else if (controller.fuelAmount < 0.6f) nitroBar.color = Color32.Lerp(yellowColor, greenColor, nitroBarCurve.Evaluate((controller.fuelAmount - 0.2f) * 2.5f));
        else nitroBar.color = greenColor;

        if (timeCounter < refreshTime)
        {
            timeCounter += Time.smoothDeltaTime;
            frameCounter++;
        }
        else
        {
            fps = frameCounter / timeCounter;
            frameCounter = 0;
            timeCounter = 0.0f; 
        }

        debugLeft.text = @$" fps: {(int)fps}
 pos: {Mathf.Round(controller.transform.position.x)} / {Mathf.Round(controller.transform.position.y)} / {Mathf.Round(controller.transform.position.z)}
 dtn: {Mathf.Round(controller.GetMinDistance())}";

        if (!infoMessage.gameObject.activeInHierarchy && messages.Count > 0)
        {
            infoMessage.text = messages.Dequeue();
            infoMessage.gameObject.SetActive(true);
            StartCoroutine(HideInfoText());
        }
    }

    IEnumerator HideInfoText()
    {
        yield return new WaitForSeconds(2f);
        infoMessage.gameObject.SetActive(false);
    }

    public void AddMessage(string message)
    {
        messages.Enqueue(message);
    }
    
    public void DisplayStats(Stats stats)
    {
        debugRight.text = stats.ToString();
    }
}
