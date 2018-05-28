using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class QualitySetter : MonoBehaviour {

	public Slider mySlider;

	// Use this for initialization
	void Start () {
		//print ("this called");
		mySlider.value = PlayerPrefs.GetInt ("Quality", 5);
		QualitySettings.SetQualityLevel(PlayerPrefs.GetInt ("Quality", 5), true);
		//print (PlayerPrefs.GetInt ("Quality", 5));
		Invoke ("Later", 0.1f);
	}

	void Later () {
		QualitySettings.SetQualityLevel(PlayerPrefs.GetInt ("Quality", 5), true);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ValueChanged () {


		QualitySettings.SetQualityLevel((int)mySlider.value, true);
		PlayerPrefs.SetInt ("Quality", (int)mySlider.value);
	}
}
