using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerMaster : MonoBehaviour {
	
	public string PlayerName = "Player";
	
	// Use this for initialization
	void Start () {
		PlayerName += " " + gameObject.GetComponent<NetworkIdentity>().playerControllerId.ToString();
	}
}
