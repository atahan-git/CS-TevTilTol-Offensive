using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class MultDeathController : NetworkBehaviour {

	bool isDead = false;

	DeathControllerMultRelay dr;


	public MonoBehaviour[] stuffToDisableOnDeath;

	void Start (){
		dr = GetComponent<PlayerRelay> ().myPlayer.GetComponent<DeathControllerMultRelay> ();
	}

	// Update is called once per frame
	void Update () {
		if (isServer) {
			if (GetComponent<Hp> ().hpi < 0 && !isDead) {
				isDead = true;
				if (GetComponent<Hp> ().myDamagers.Count > 0) {
					GetComponent<Hp> ().myDamagers = GetComponent<Hp> ().myDamagers.Where (item => item != null).ToList ();
					GetComponent<Hp> ().myDamagers = GetComponent<Hp> ().myDamagers.Distinct ().ToList ();
					if (GetComponent<Hp> ().myDamagers.Count > 0) {
						foreach (int id in GetComponent<Hp> ().myDamagers) {
							if (id != null)
								ScoreBoard.s.a [id] += 1;
						}
					}
				}
				if (GetComponent<Hp> ().lastDamager != -1)
					ScoreBoard.s.k [GetComponent<Hp> ().lastDamager] += 1;

				ScoreBoard.s.d [GetComponent<PlayerRelay> ().myId] += 1;
				Die ();
			}
		}

		if (isLocalPlayer) {
			if (isDead) {
				dr.Update ();
			}
		}
	}

	public float reviveTime = 7f;

	void Die (){
		GameObject myItem = (GameObject)Instantiate(STORAGE_Explosions.s.itemdrop, transform.position + (Vector3.up * 2), transform.rotation);
		NetworkServer.Spawn (myItem);
		if (myItem.GetComponent<GunDrop> ()) {
			myItem.GetComponent<GunDrop> ().MakeGun (GetComponent<GunBuilder>().myGun);
		}



		RpcDie ();
		Invoke ("Revive",reviveTime);
	}

	[ClientRpc]
	void RpcDie (){
		Instantiate (STORAGE_Explosions.s.bigExp, transform.position, transform.rotation);
		isDead = true;
		if (isLocalPlayer) {
			LocalStuffToggle (false);
			GetComponent<PlayerRelay> ().myPlayer.SetActive (false);
			DeathControllerMultRelay.s.ToggleDeath (true,reviveTime);
			GetComponent<PlayerRelay> ().startPos = SpawnLocations.s.GetSpawnLocation ().position;
			GetComponent<PlayerRelay> ().myPlayer.transform.position = GetComponent<PlayerRelay> ().startPos + Vector3.down * 20;
			GetComponent<PlayerRelay> ().myPlayer.transform.GetChild (0).rotation = Quaternion.identity;
		} else {
			transform.Find ("enemyVisuals").gameObject.SetActive(false);
			transform.Find ("GunBasePos").gameObject.SetActive(false);
		}
	}

	[ClientRpc]
	void RpcRevive (Vector3 loc){
		isDead = false;
		GetComponent<PlayerRelay> ().startPos = loc;
		if (isLocalPlayer) {
			Instantiate (STORAGE_Explosions.s.spawnEffect, transform.position, Quaternion.identity);
			LocalStuffToggle (true);
			GetComponent<PlayerRelay> ().myPlayer.transform.position = GetComponent<PlayerRelay> ().startPos;
			transform.position = GetComponent<PlayerRelay> ().myPlayer.transform.position;
			GetComponent<GunBuilder> ().myGun.gunLevel = 1;
			GetComponent<GunBuilder> ().myGun.gunRarity = 0;
			GetComponent<GunBuilder> ().RandomizeGunParts ();
			GetComponent<GunBuilder> ().SetGunParts ();
			GetComponent<GunBuilder> ().BuildGun ();
			GetComponent<PlayerRelay> ().myPlayer.SetActive (true);
			DeathControllerMultRelay.s.ToggleDeath (false,reviveTime);
		} else {
			Instantiate (STORAGE_Explosions.s.spawnEffect, transform.position, Quaternion.identity);
			transform.Find ("enemyVisuals").gameObject.SetActive(true);
			transform.Find ("GunBasePos").gameObject.SetActive(true);
		}
	}


	void Revive (){
		GetComponent<Hp> ().hpi = GetComponent<Hp> ().maxhp;
		isDead = false;
		RpcRevive (SpawnLocations.s.GetSpawnLocation ().position);
	}

	public void LocalStuffToggle(bool toggle){
		foreach (MonoBehaviour mono in stuffToDisableOnDeath) {
			if (mono != null) {
				mono.enabled = toggle;
			}
		}
	}
}
