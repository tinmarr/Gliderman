using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSounds : MonoBehaviour
{
    // Start is called before the first frame update
    SoundManager soundManager;
    void Start()
    {
        soundManager = FindObjectOfType<SoundManager>();
    }
    public void OnHover()
    {
        soundManager.OnHover();
    }
    public void OnClick()
    {
        soundManager.OnClick();
    }

}
