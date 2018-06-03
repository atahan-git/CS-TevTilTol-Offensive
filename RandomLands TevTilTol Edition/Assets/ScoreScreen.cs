using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreScreen : MonoBehaviour {

	public static ScoreScreen s;

	public GameObject screen;

	// Use this for initialization
	void Awake () {
		s = this;
		screen.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Tab)) {
			screen.SetActive (true);
		}

		if (Input.GetKeyUp (KeyCode.Tab)) {
			screen.SetActive (false);
		}
	}
}
