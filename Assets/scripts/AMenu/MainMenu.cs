using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip enterSound;
    public AudioClip clickSound;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void NewGame()
    {
        // get the seed from config
     }
    public void Quit()
    {
        Application.Quit();
    }
    public void OnHover()
    {
        audioSource.PlayOneShot(enterSound);
    }
    public void OnClick()
    {
        audioSource.PlayOneShot(clickSound);
    }
}
