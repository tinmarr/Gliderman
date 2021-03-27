using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    AudioSource audioSource;
    public AudioClip enterSound;
    public AudioClip clickSound;
    // Start is called before the first frame update
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
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
