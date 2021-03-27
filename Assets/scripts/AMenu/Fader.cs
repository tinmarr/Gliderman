using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fader : MonoBehaviour
{
    public float duration;
    public bool fadedState;
    private void Start()
    {
        
    }
    public void Fade()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        StartCoroutine(DoFade(canvasGroup, canvasGroup.alpha, fadedState ? 1 : 0));
        fadedState = !fadedState;
    }
    public void turnOff()
    {
        fadedState = true;
        GetComponent<CanvasGroup>().alpha = 0;

    }
    public IEnumerator DoFade(CanvasGroup canvas, float start, float end)
    {
        float counter = 0f;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            canvas.alpha = Mathf.Lerp(start, end, counter / duration);

            yield return null;
        }

    }
}
