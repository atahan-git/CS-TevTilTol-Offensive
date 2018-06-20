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

	[SyncVar]
	public int myId = -1;

	public GameObject GunParent;

	public Vector3 startPos;

	[SyncVar]
	public string myName = "player";

	// Use this for initialization
	void Awake () {

		myPlayer = GameObject.FindGameObjectWithTag ("Player");
		myPlayer.GetComponentInChildren<AudioListener> ().enabled = false;

		GunBase = transform.GetChild (0).gameObject;
		startPos = myPlayer.transform.position;
		transform.position = new Vector3 (0, -5, 0);

	}
		


	void Start () {
		if (isLocalPlayer) {
			myName = PlayerPrefs.GetString ("name", "player_" + Random.Range (0, 1000));
			ToggleEnemyVisuals (false);
			gameObject.name = "Local " + gameObject.name;
			localRelay = this;
			GunParent.SetActive(false);
			myPlayer.transform.position = new Vector3 (0, -5, 0);
			foreach (Camera cam in myPlayer.GetComponentsInChildren<Camera>())
				cam.enabled = false;

			CmdRegisterScoreboard (myName);

			Invoke ("DisableStuff", 0.5f);
		} else {
			ToggleEnemyVisuals (true, GetComponent<Hp>().mySide);
		}
	}

	[Command]
	void CmdRegisterScoreboard (string name){
		myName = name;
		myId = ScoreBoard.s.AddPlayer (myName);

		RpcGetId (myId);
	}

	[ClientRpc]
	void RpcGetId (int theId) {
		myId = theId;
	}


	void DisableStuff (){
		ToggleEnemyVisuals (false);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		myPlayer.SetActive (false);
		myPlayer.transform.position = new Vector3 (0, -5, 0);
		ToggleStartDisable (false);
		TeamSelectionMenu.s.ActivateMenu (true);
		TeamSelectionMenu.s.DisableFillerCam ();
	}

	public void TeamSelected (int teamId, int enemyType){
		myPlayer.SetActive (true);
		myPlayer.GetComponentInChildren<AudioListener> ().enabled = true;
		foreach (Camera cam in myPlayer.GetComponentsInChildren<Camera>())
			cam.enabled = true;
		startPos = SpawnLocations.s.GetSpawnLocation ().position;
		myPlayer.transform.position = startPos;
		transform.position = startPos;
		TeamSelectionMenu.s.ActivateMenu (false);
		CmdTeamSelected (teamId, enemyType, startPos);
		ToggleStartDisable (true);

		Instantiate (STORAGE_Explosions.s.spawnEffect, transform.position, Quaternion.identity);
		if (!Application.isEditor) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	[Command]
	void CmdTeamSelected (int teamId, int enemyType, Vector3 loc){
		startPos = loc;
		GetComponent<Hp> ().mySide = teamId;
		myEnemyType = enemyType;
		RpcEnemyType (teamId, enemyType, startPos);
		if (!isLocalPlayer) {
			ToggleEnemyVisuals (true, teamId);
		}
	}

	[ClientRpc]
	void RpcEnemyType (int teamId, int enemyType, Vector3 loc){
		startPos = loc;
		print ("Enemy team color set");
		myEnemyType = enemyType;
		transform.position = startPos;
		if (!isLocalPlayer) {
			Instantiate (STORAGE_Explosions.s.spawnEffect, transform.position, Quaternion.identity);
			ToggleEnemyVisuals (true, teamId);
		}
	}

		

	float pingTimer = 0f;
	float pingDelay = 1f;
	// Update is called once per frame
	void Update () {
		
		if (isLocalPlayer) {
			transform.position = myPlayer.transform.position;
			transform.rotation = Quaternion.Euler (new Vector3 (transform.rotation.eulerAngles.x, myPlayer.transform.GetChild (0).rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
			GunBase.transform.rotation = myPlayer.transform.GetChild (0).rotation;
			gunRot = GunBase.transform.rotation;
			Health.s.health = GetComponent<Hp> ().hpi;
			Health.s.maxHealth = GetComponent<Hp> ().maxhp;

			if (pingTimer < 0) {
				ScoreBoard.s.ping [myId] = Network.GetAveragePing (Network.player);
				pingTimer = pingDelay;
			}

			pingTimer -= Time.deltaTime;
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
		//GetComponent<Hp>().healthBar = enemyVisuals [myEnemyType].GetComponentInChildren<UnityEngine.UI.Slider> ();
		//print (gameObject.name +  " - HealthBar Set - " + enemyVisuals [myEnemyType].GetComponentInChildren<UnityEngine.UI.Slider> ().transform.parent.parent + " ---- " + enemyVisuals [myEnemyType].gameObject.name);
	}

	void ToggleEnemyVisuals (bool toggle, int teamId){
		foreach (ColorableMaterial mat in enemyVisuals [myEnemyType].GetComponentsInChildren<ColorableMaterial> ()) {
			mat.SetColor (teamId);
		}
		ToggleEnemyVisuals (toggle);
	}

	void ToggleStartDisable (bool toggle){
		foreach (MonoBehaviour gm in startDis) {
			if (gm != null)
				gm.enabled = toggle;
		}
	}
}
