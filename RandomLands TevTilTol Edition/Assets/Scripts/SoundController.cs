using UnityEngine;
using System.Collections;

public class SoundController : MonoBehaviour {

	public AudioSource[] musicSources;
	public AudioSource[] voiceSources;

	// Use this for initialization
	void Start () {
	
	}

	float oldSound = -1f;
	float oldMusic = -1f;
	float oldVoice = -1f;
	
	// Update is called once per frame
	void Update () {

		float newSound = PlayerPrefs.GetFloat ("SoundVolume", 1);
		float newMusic = PlayerPrefs.GetFloat ("MusicVolume", 0.5f);
		float newVoice = PlayerPrefs.GetFloat ("VoiceVolume", 1f);

		if (oldSound != newSound) {
			AudioListener.volume = newSound;
		}

		if (oldSound != newSound || oldMusic != newMusic) {
			foreach (AudioSource aud in musicSources) {
				aud.volume = newMusic / newSound;
			}
		}

		if (oldSound != newSound || oldVoice != newVoice) {
			foreach (AudioSource aud in voiceSources) {
				if(aud != null)
				aud.volume = newVoice / newSound;
			}
		}

		oldSound = newSound;
		oldMusic = newMusic;
		oldVoice = newVoice;
    }
}
