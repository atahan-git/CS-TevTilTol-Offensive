using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerRelay : NetworkBehaviour {

	public static PlayerRelay localRelay;

	public GameObject myPlayer;

	public GameObject[] enemyVisuals;
	public MonoBehaviour[] startDis;

	[SyncVar]
	public int myEnemyType = 0;

	// Use this for initialization
	void Awake () {
		myPlayer = GameObject.FindGameObjectWithTag ("Player");
	}
		


	void Start () {
		if (isLocalPlayer) {
			ToggleEnemyVisuals (false);
			gameObject.name = "Local " + gameObject.name;
			localRelay = this;

			Invoke ("DisableStuff", 0.5f);
			TeamSelectionMenu.s.ActivateMenu (true);
		} else {
			ToggleEnemyVisuals (true, GetComponent<Hp>().mySide);
		}
	}

	Vector3 startPos;

	void DisableStuff (){
		ToggleEnemyVisuals (false);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		myPlayer.SetActive (false);
		startPos = myPlayer.transform.position;
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
			Health.s.health = GetComponent<Hp> ().hpi;
		} else {


		}
	}

	void ToggleEnemyVisuals (bool toggle){
		foreach (GameObject gm in enemyVisuals) {
			if (gm != null)
				gm.SetActive (false);
		}

		enemyVisuals [myEnemyType].SetActive (toggle);
	}

	void ToggleEnemyVisuals (bool toggle, int teamId){
		foreach (GameObject gm in enemyVisuals) {
			if (gm != null)
				gm.SetActive (false);
		}

		enemyVisuals [myEnemyType].SetActive (toggle);
		enemyVisuals[myEnemyType].GetComponentInChildren<MeshRenderer>().material.color = teamId == 0 ? Color.red : Color.blue;
	}

	void ToggleStartDisable (bool toggle){
		foreach (MonoBehaviour gm in startDis) {
			if (gm != null)
				gm.enabled = toggle;
		}
	}
}
