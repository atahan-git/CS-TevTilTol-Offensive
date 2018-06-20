using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerRelay : MonoBehaviour {

	public static NetworkManagerRelay s;

	UnityEngine.Networking.NetworkManager myManager;

	public enum States {Host, Server, Client}
	public States myState;

	public bool isConnected;

	// Use this for initialization
	void Start () {
		if (s == null)
			s = this;
		else if (s != this)
			Destroy (gameObject);

		myManager = GetComponent<UnityEngine.Networking.NetworkManager> ();
	}


	public void SetIp (string id){
		myManager.networkAddress = id;
	}
	
	public void HostGame (){
		if (isConnected == false) {
			myManager.StartHost ();
			isConnected = true;
			myState = States.Host;
		}
	}

	public void JoinGame (){
		if (isConnected == false) {
			myManager.StartClient ();
			isConnected = true;
			myState = States.Client;
		}
	}

	public void Disconnect (){
		if(isConnected == true){
		isConnected = false;
			switch (myState) {
			case States.Client:
				myManager.StopClient ();
				break;
			case States.Host:
				myManager.StopHost ();
				break;
			case States.Server:
				myManager.StopServer ();
				break;
			}

		}

	}
}
