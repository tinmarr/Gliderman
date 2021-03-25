using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetFPS : MonoBehaviour
{
    Text text;
    int m_frameCounter = 0;
    float m_timeCounter = 0.0f;
    float fps = 0.0f;
    public float m_refreshTime = 0.5f;

    void Start()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        if (m_timeCounter < m_refreshTime)
        {
            m_timeCounter += Time.smoothDeltaTime;
            m_frameCounter++;
        }
        else
        {
            //This code will break if you set your m_refreshTime to 0, which makes no sense.
            fps = (float)m_frameCounter / m_timeCounter;
            m_frameCounter = 0;
            m_timeCounter = 0.0f;
        }

        text.text = "FPS: " + (int)(fps);
    }
}
