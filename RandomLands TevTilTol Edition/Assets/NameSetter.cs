using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameSetter : MonoBehaviour {

	InputField myField;

	// Use this for initialization
	void Start () {
		myField = GetComponent<InputField> ();
		myField.text = PlayerPrefs.GetString ("name", "player_" + Random.Range (0, 1000));
	}

	public void SetName (){
		PlayerPrefs.SetString ("name", myField.text);
	}
}
