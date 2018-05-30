using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerRelay : NetworkBehaviour {

	public static PlayerRelay localRelay;

	public GameObject myPlayer;
	GameObject GunBase;

	public GameObject[] enemyVisuals;
	public MonoBehaviour[] startDis;

	[SyncVar]
	public int myEnemyType = 0;

	[SyncVar]
	Quaternion gunRot;

	public GameObject GunParent;

	public Vector3 startPos;

	// Use this for initialization
	void Awake () {
		myPlayer = GameObject.FindGameObjectWithTag ("Player");
		GunBase = transform.GetChild (0).gameObject;
		startPos = myPlayer.transform.position;
		transform.position = new Vector3 (0, -5, 0);
	}
		


	void Start () {
		if (isLocalPlayer) {
			ToggleEnemyVisuals (false);
			gameObject.name = "Local " + gameObject.name;
			localRelay = this;
			GunParent.SetActive(false);
			myPlayer.transform.position = new Vector3 (0, -5, 0);

			Invoke ("DisableStuff", 0.5f);
			TeamSelectionMenu.s.ActivateMenu (true);
		} else {
			ToggleEnemyVisuals (true, GetComponent<Hp>().mySide);
		}
	}


	void DisableStuff (){
		ToggleEnemyVisuals (false);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		myPlayer.SetActive (false);
		myPlayer.transform.position = new Vector3 (0, -5, 0);
		ToggleStartDisable (false);
	}

	public void TeamSelected (int teamId, int enemyType){
		myPlayer.SetActive (true);
		myPlayer.transform.position = startPos;
		TeamSelectionMenu.s.ActivateMenu (false);
		CmdTeamSelected (teamId, enemyType);
		ToggleStartDisable (true);
		if (!Application.isEditor) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	[Command]
	void CmdTeamSelected (int teamId, int enemyType){
		GetComponent<Hp> ().mySide = teamId;
		myEnemyType = enemyType;
		RpcEnemyType (teamId, enemyType);
		if (!isLocalPlayer) {
			ToggleEnemyVisuals (true, teamId);
		}
	}

	[ClientRpc]
	void RpcEnemyType (int teamId, int enemyType){
		print ("Enemy team color set");
		myEnemyType = enemyType;
		if (!isLocalPlayer) {
			ToggleEnemyVisuals (true, teamId);
		}
	}

		

	
	// Update is called once per frame
	void Update () {

		if (isLocalPlayer) {
			transform.position = myPlayer.transform.position;
			transform.rotation = Quaternion.Euler (new Vector3 (transform.rotation.eulerAngles.x, myPlayer.transform.GetChild (0).rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
			GunBase.transform.rotation = myPlayer.transform.GetChild (0).rotation;
			gunRot = GunBase.transform.rotation;
			Health.s.health = GetComponent<Hp> ().hpi;
			Health.s.maxHealth = GetComponent<Hp> ().maxhp;
		} else {
			GunBase.transform.rotation = gunRot;
		}
	}

	void ToggleEnemyVisuals (bool toggle){
		foreach (GameObject gm in enemyVisuals) {
			if (gm != null)
				gm.SetActive (false);
		}

		enemyVisuals [myEnemyType].SetActive (toggle);
		GetComponent<Hp>().healthBar = enemyVisuals [myEnemyType].GetComponentInChildren<UnityEngine.UI.Slider> ();
		//print (gameObject.name +  " - HealthBar Set - " + enemyVisuals [myEnemyType].GetComponentInChildren<UnityEngine.UI.Slider> ().transform.parent.parent + " ---- " + enemyVisuals [myEnemyType].gameObject.name);
	}

	void ToggleEnemyVisuals (bool toggle, int teamId){
		enemyVisuals[myEnemyType].GetComponentInChildren<MeshRenderer>().material.color = teamId == 0 ? Color.red : Color.blue;
		ToggleEnemyVisuals (toggle);
	}

	void ToggleStartDisable (bool toggle){
		foreach (MonoBehaviour gm in startDis) {
			if (gm != null)
				gm.enabled = toggle;
		}
	}
}
