using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DifficultyText : MonoBehaviour {

	Text myText;

	// Use this for initialization
	void Start () {
		myText = GetComponent<Text> ();

		switch (PlayerPrefs.GetInt ("Diff", -1)) {
		case 0:
			myText.text = (PlayerPrefs.GetInt("Lang",0) == 0) ? "Difficulty: Easy" : "Zorluk: Kolay";
			break;
		case 1:
			myText.text = (PlayerPrefs.GetInt("Lang",0) == 0) ? "Difficulty: Normal/Kind" : "Zorluk: Normal/Yumuşak";
			break;
		case 2:
			myText.text = (PlayerPrefs.GetInt("Lang",0) == 0) ? "Difficulty: Normal" : "Zorluk: Normal";
			break;
		case 3:
			myText.text = (PlayerPrefs.GetInt("Lang",0) == 0) ? "Difficulty: Normal/Harsh" : "Zorluk: Normal/Zorlu";
			break;
		case 4:
			myText.text = (PlayerPrefs.GetInt("Lang",0) == 0) ? "Difficulty: Hardcore" : "Zorluk: Hardcore";
			break;
		default:
			myText.text = "error";
			break;
		}
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
