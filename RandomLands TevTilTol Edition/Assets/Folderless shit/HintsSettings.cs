using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HintsSettings : MonoBehaviour {

	Toggle myTog;

	// Use this for initialization
	void OnEnable () {
		myTog = GetComponent<Toggle> ();
		if (PlayerPrefs.GetInt ("HintsEnabled", 1) == 1) {
			myTog.isOn = true;
		} else {
			myTog.isOn = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public bool debug = false;

	public void SetHints (bool val){
		debug = myTog.isOn;
		if (myTog.isOn) {
			PlayerPrefs.SetInt ("HintsEnabled", 1);
		} else {
			PlayerPrefs.SetInt ("HintsEnabled", 0);
		}
	}
}
