using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameBenderCrossHair : MonoBehaviour {

	public static GameBenderCrossHair s;

	Image img;

	// Use this for initialization
	void Awake () {
		s = this;

		img = GetComponent<Image> ();
	}

	// Update is called once per frame
	void Update () {
	
	}

	public void CanShoot (bool state){

		if (state) {
			img.color = Color.red;
		} else {
			img.color = Color.white;
		}

	}
}
