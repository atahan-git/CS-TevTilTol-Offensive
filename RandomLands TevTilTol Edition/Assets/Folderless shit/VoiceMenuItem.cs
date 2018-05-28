using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class VoiceMenuItem : MonoBehaviour {

	public Slider slider;

	float curSound = -2f;

	// Use this for initialization
	void OnEnable()
	{
		slider = GetComponentInChildren<Slider> ();
		curSound = PlayerPrefs.GetFloat("VoiceVolume", 1f);
		slider.value = curSound;
	}

	// Update is called once per frame
	void Update()
	{

		if (curSound < 0)
			return;
		curSound = slider.value;
		PlayerPrefs.SetFloat("VoiceVolume", curSound);
	}
}
