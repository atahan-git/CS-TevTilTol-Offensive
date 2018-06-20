using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class IpAdressDisplay : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if (NetworkManagerRelay.s) {
			if (NetworkManagerRelay.s.myState == NetworkManagerRelay.States.Host || NetworkManagerRelay.s.myState == NetworkManagerRelay.States.Server) {
				GetComponent<Text> ().text = "Hosting: " + Network.player.ipAddress;
			} else {
				GetComponent<Text> ().enabled = false;
			}
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
