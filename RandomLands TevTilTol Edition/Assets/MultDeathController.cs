using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MultDeathController : NetworkBehaviour {

	bool isDead = false;

	// Update is called once per frame
	void Update () {
		if (isServer) {
			if (GetComponent<Hp> ().hpi < 0 && !isDead) {
				isDead = true;
				Die ();
			}
		}
	}

	void Die (){
		GameObject myItem = (GameObject)Instantiate(STORAGE_Explosions.s.itemdrop, transform.position + (Vector3.up * 2), transform.rotation);
		NetworkServer.Spawn (myItem);
		if (myItem.GetComponent<GunDrop> ()) {
			myItem.GetComponent<GunDrop> ().MakeGun (GetComponent<GunBuilder>().myGun);
		}

		RpcDie ();
		Invoke ("Revive",1f);
	}

	[ClientRpc]
	void RpcDie (){
		Instantiate (STORAGE_Explosions.s.bigExp, transform.position, transform.rotation);
		isDead = true;
		if (isLocalPlayer) {
			DeathControllerMultRelay.s.ToggleDeath (true);
			GetComponent<PlayerRelay> ().myPlayer.transform.position = GetComponent<PlayerRelay> ().startPos;
			GetComponent<PlayerRelay> ().myPlayer.transform.GetChild (0).rotation = Quaternion.identity;
		} else {
			transform.Find ("enemyVisuals").gameObject.SetActive(false);
			transform.Find ("GunBasePos").gameObject.SetActive(false);
		}
	}

	[ClientRpc]
	void RpcRevive (){
		isDead = false;
		if (isLocalPlayer) {
			DeathControllerMultRelay.s.ToggleDeath (false);
		} else {
			transform.Find ("enemyVisuals").gameObject.SetActive(true);
			transform.Find ("GunBasePos").gameObject.SetActive(true);
		}
	}


	void Revive (){
		GetComponent<Hp> ().hpi = GetComponent<Hp> ().maxhp;
		isDead = false;
		RpcRevive ();
	}
}
