using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisconnectRelay : MonoBehaviour {

	public GameObject uSureMenu;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	public void StartDisconnect () {
		uSureMenu.SetActive (true);
	}


	public void Yes (){
		NetworkManagerRelay.s.Disconnect ();
	}

	public void No (){
		uSureMenu.SetActive (false);
	}
}
