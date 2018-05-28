using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SoundMenuItem : MonoBehaviour {

    public Slider slider;

    float curSound = -2f;

    // Use this for initialization
	void OnEnable () {
		slider = GetComponentInChildren<Slider> ();
        curSound = PlayerPrefs.GetFloat("SoundVolume", 1);
        slider.value = curSound;
	}
	
	// Update is called once per frame
	void Update () {

        if (curSound < 0)
            return;
        curSound = slider.value;
        PlayerPrefs.SetFloat("SoundVolume", curSound);
	}
}
