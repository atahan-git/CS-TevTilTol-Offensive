using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;
using UnityEngine.Networking;

public class WeaponDropBoxController : NetworkBehaviour {

	public GameObject dropPosition;

	public AudioClip openSound;
	public AudioClip readySound;

	public Light disabledLight;
	public Light readyLight;

	public Animator anim;
	public AudioSource aud;

	[SyncVar]
	public bool isReady = false;
	//Vector2 readyTime = new Vector2 (6f,8f);

	Outline[] myOutlines;
	// Use this for initialization
	void Start () {
		myOutlines = GetComponentsInChildren<Outline> ();
		WeaponDropMaster.s.myBoxes.Add (this);

		if (isServer) {
			if (isReady) {
				GetReady ();
			} else {
				SetReadyState (false);
				//Invoke ("GetReady", Random.Range (readyTime.x, readyTime.y));
			}
		}

		if (!isServer) {
			SetReadyState (isReady);
		}
	}

	public void GetReady(){
		isReady = true;
		RpcGetReady ();
	}

	[ClientRpc]
	void RpcGetReady(){
		SetReadyState (true);
	}
		
	public void Activate (){
		if (isReady) {
			isReady = false;
			RpcActivate ();
			Invoke ("DropItem", 1.1f);
			//Invoke ("GetReady", Random.Range (readyTime.x, readyTime.y));
		}
	}

	[ClientRpc]
	void RpcActivate (){
		anim.SetBool ("isOpen",true);
		aud.Stop ();
		aud.volume = 1f;
		aud.pitch = 1f;
		aud.clip = openSound;
		aud.Play ();
		Invoke ("CloseBack",3f);
	}

	void CloseBack (){
		SetReadyState (false);
	}

	void DropItem (){
		GameObject myItem = (GameObject)Instantiate(STORAGE_Explosions.s.itemdrop, transform.position + Vector3.up, transform.rotation);
		NetworkServer.Spawn (myItem);
		int gunLevel = Random.Range (1, 6); //1-5
		int gunRarity = 0;
		/*gunRarity = gunLevel-((int)Mathf.Sqrt(Random.Range(0,gunLevel*gunLevel)));
		gunRarity = Mathf.Clamp (gunRarity,0,gunLevel);*/
		if (myItem.GetComponent<GunDrop>())
			myItem.GetComponent<GunDrop>().MakeGun(gunLevel, gunRarity);

		RpcDropEffect (myItem);
		myItem.GetComponent<Rigidbody> ().AddForce ((transform.TransformDirection ((Vector3.forward + (Random.Range(-1f,1f)*Vector3.left))).normalized + (Vector3.up * 2)) * 200);
		myItem.GetComponent<Rigidbody> ().AddTorque (Random.Range(15,100), Random.Range(15,100), Random.Range(15,100));
	}

	[ClientRpc]
	void RpcDropEffect (GameObject gun){
		Instantiate(STORAGE_Explosions.s.normalExp, transform.position + Vector3.up, transform.rotation);

		if (!isServer) {
			gun.GetComponent<Rigidbody> ().AddForce ((transform.TransformDirection (Vector3.forward) + (Vector3.up * 2)) * 200);
			gun.GetComponent<Rigidbody> ().AddTorque (Random.Range (15, 100), Random.Range (15, 100), Random.Range (15, 100));
		}
	}

	void SetReadyState (bool state){
		SetOutlineState (state);
		aud.Stop ();
		if (state) {
			readyLight.enabled = true;
			disabledLight.enabled = false;
			aud.volume = 1f;
			aud.pitch = 1f;
			aud.clip = readySound;
			aud.Play ();
			GetComponent<AppearOnCanPickUp> ().enabled = true;

		} else {
			GetComponent<AppearOnCanPickUp> ().enabled = false;
			readyLight.enabled = false;
			disabledLight.enabled = true;
			aud.volume = 0.8f;
			aud.pitch = 0.8f;
			aud.clip = openSound;
			aud.Play ();
			anim.SetBool ("isOpen",false);
		}
	}
		
	void SetOutlineState (bool state){
		foreach (Outline mono in myOutlines) {
			if (mono != null)
				mono.enabled = state;
		}
	}
}
