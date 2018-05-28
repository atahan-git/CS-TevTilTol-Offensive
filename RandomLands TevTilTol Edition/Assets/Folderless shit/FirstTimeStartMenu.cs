using UnityEngine;
using System.Collections;

public class FirstTimeStartMenu : MonoBehaviour {

	public GameObject myMenu;

	// Use this for initialization
	void Start () {
		if (PlayerPrefs.GetInt ("fstart", 1) == 1) {
			myMenu.SetActive (true);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void CloseMenu(){
		myMenu.SetActive (false);
		PlayerPrefs.SetInt ("fstart", 0);
	}
}
