using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ResetAll : MonoBehaviour {

	public Text myText;

	public bool isOnce;

	// Use this for initialization
	void OnEnable () {
		GetComponentInChildren<LangTextChanger> ().UpdateText ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Click () {

		if (!isOnce) {
			if (PlayerPrefs.GetInt ("Lang", 0) == 0) {
				//en
				myText.text = "Are You Sure?";
			} else {
				//tr
				myText.text = "Emin Misin?";
			}
			isOnce = true;
		} else {
			ResetScores ();
		}
	}

	void ResetScores() {
		PlayerPrefs.DeleteAll ();
		MenuController.s.ReloadOptions ();
	}
}
