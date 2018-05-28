using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HighScoreDisplayer : MonoBehaviour {

	Text myText;

	// Use this for initialization
	void Awake () {
		myText = GetComponent<Text> ();

		SetScores ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetScores(){

		myText.text = "High Score: " + PlayerPrefs.GetInt("HScore", 0).ToString() + "\n" 
			+ "Code: " + PlayerPrefs.GetInt ("HScoreSec", 0).ToString();
	}
}
