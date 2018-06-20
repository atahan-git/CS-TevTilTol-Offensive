using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreScreen : MonoBehaviour {

	public static ScoreScreen s;

	public GameObject screen;

	public GameObject scoreParent;
	public GameObject scorePrefab;

	// Use this for initialization
	void Awake () {
		s = this;
		screen.SetActive (false);
	}

	void Start (){
		int _count = ScoreBoard.s.nicks.Count;

		for (int i = 0; i < _count; i++) {
			AddPlayer (i);
		}
	}

	int count = 0;
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Tab)) {
			screen.SetActive (true);
		}

		if (Input.GetKeyUp (KeyCode.Tab)) {
			screen.SetActive (false);
		}

		int _count = ScoreBoard.s.nicks.Count;
		if (count < _count) {
			for (int i = count; i < _count; i++) {
				AddPlayer (i);
			}
		}
	}


	public void AddPlayer (int ID){
		count++;
		Instantiate (scorePrefab, scoreParent.transform).GetComponent<ScoreKeeper>().myId = ID;
	}

}
