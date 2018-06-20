using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IpSetter : MonoBehaviour {

	InputField myField;

	// Use this for initialization
	void Start () {
		myField = GetComponent<InputField> ();
		myField.text = PlayerPrefs.GetString ("ip", "10.100.1x.xxx");
	}

	public void SetIp (){
		NetworkManagerRelay.s.SetIp (myField.text);
		PlayerPrefs.SetString ("ip", myField.text);
	}
}
