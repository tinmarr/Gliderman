using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CelestialController : MonoBehaviour
{
    [Tooltip("Length of a day in seconds")]
    public int dayTime = 5 * 60;

    public int timeOfDay = 0;

    public int forcedToD = -1;

    float dayLength = 24 * 60f;
    float angle = 0;
    Light sunlight;

    private void Start()
    {
        sunlight = GetComponentInChildren<Light>();
        StartCoroutine(TimeKeeper());
    }

    private void Update()
    {
        if (forcedToD != -1)
        {
            timeOfDay = forcedToD;
        }

        angle = Mathf.Lerp(0, 360, timeOfDay / dayLength);

        sunlight.transform.rotation = Quaternion.Euler(new Vector3(angle, -90, 0));
    }

    IEnumerator TimeKeeper()
    {
        yield return new WaitForSeconds(dayTime / dayLength);

        timeOfDay++;
        if (timeOfDay > dayLength) timeOfDay = 0;

        StartCoroutine(TimeKeeper());
    }
}
