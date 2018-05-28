using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MultDeathController : NetworkBehaviour {

	bool isDead = false;

	// Update is called once per frame
	void Update () {

		if (GetComponent<Hp> ().hpi < 0 && !isDead) {
			isDead = true;
			Die (true);
		}
	}

	void Die (bool toggle){
		if (isLocalPlayer) {
			DeathControllerMultRelay.s.ToggleDeath (toggle);
		} else {
			transform.GetChild (0).gameObject.SetActive(!toggle);
		}

		if(toggle)
			Invoke ("Revive",1f);
	}


	void Revive (){
		GetComponent<Hp> ().hpi = GetComponent<Hp> ().maxhp;
		isDead = false;
		Die (false);
	}
}
