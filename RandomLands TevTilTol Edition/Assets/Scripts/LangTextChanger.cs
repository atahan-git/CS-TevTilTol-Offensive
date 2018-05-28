using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LangTextChanger : MonoBehaviour {

	[TextArea]
	public string TR;
	public AudioClip trClip;
	[TextArea]
	public string EN;
	public AudioClip enClip;

	Text myText;
	Dialog myDialog;

	// Use this for initialization
	void Awake () {
		myText = GetComponent<Text> ();
		myDialog = GetComponent<Dialog> ();
		if (myText == null && myDialog == null) {
			Debug.LogError ("Text not Found");
			return;
		}

		UpdateText ();
	}

	public void UpdateText () {
		
		if (PlayerPrefs.GetInt ("Lang", 0) == 0) {
			if (myText)
				myText.text = EN;
			else if (myDialog) {
				myDialog.text = EN;
				if (enClip)
					myDialog.soundFile = enClip;
			}
		} else {
			if (myText)
				myText.text = TR;
			else if (myDialog) {
				myDialog.text = TR;
				if (trClip)
					myDialog.soundFile = trClip;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
