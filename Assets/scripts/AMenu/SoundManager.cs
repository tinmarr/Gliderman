using UnityEngine.Audio;
using UnityEngine;
using System.Collections;
using System;

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

	public static SoundManager instance;

	public AudioMixerGroup mixerGroup;

	public Sound[] sounds;

	void Awake()
	{
		if (instance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}

		foreach (Sound s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.loop = s.loop;

			s.source.outputAudioMixerGroup = mixerGroup;
		}
	}
	/// <summary>
	/// plays a sound from the audiosource
	/// </summary>
	/// <param name="sound">
	/// sounds are past as strings declared in the soundmanager instance, each sound has a "name" so check there
	/// </param>
	/// <param name="setVolume">
	/// clamped between 0 and 1 you can play the sound at a certain volume
	/// </param>
	public void Play(string sound, float setVolume = 0)
	{
		setVolume = Mathf.Clamp(setVolume, 0, 1);
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		if (setVolume != 0)
		{
			s.source.volume = setVolume;
		}
		else
		{
			s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
			s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));
		}
		s.source.Play();
	}
	public void ChangeVol(string sound, float newVolume)
    {
		newVolume = Mathf.Clamp(newVolume, 0, 1);
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		if (newVolume != 0)
		{
			s.source.volume = newVolume;
		}
	}
	public float GetVol(string sound)
    {
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
        {
			Debug.LogWarning($"Sound: {name} not found!");
			return 0f;
        }
		return s.source.volume;

    }
	public void FadeOut(string sound, float seconds)
    {
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		StartCoroutine(Fade(s, seconds));
	}
	IEnumerator Fade(Sound sound, float seconds)
    {
		float decrement = 0.01f / seconds * sound.volume;
		while(sound.source.volume > 0)
        {
			sound.source.volume -= decrement;
			yield return new WaitForSeconds(0.01f);
        }
		StopSound(sound.name);
    }
	public void FadeIn(string sound, float seconds)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		Play(sound,0.01f);
		StartCoroutine(In(s, seconds));
	}
	IEnumerator In(Sound sound, float seconds)
	{
		float decrement = 0.01f / seconds * sound.volume;
		while (sound.source.volume < sound.volume)
		{
			sound.source.volume += decrement;
			yield return new WaitForSeconds(0.01f);
		}
	}
	public void StopSound(string sound)
    {
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		s.source.Stop();
	}
}
