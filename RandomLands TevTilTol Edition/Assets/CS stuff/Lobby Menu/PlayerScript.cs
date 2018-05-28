using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerScript : NetworkBehaviour {

	/*
	 * Use this script as a guideline to how to setup a player id and scoreboard system
	 * This script is NOT in active use
	 */

	[SyncVar]
	public int id = -1;

	public bool isActive = true;
	public bool canMove = true;
	public bool canSelect = true;
	public bool isPowerUp = false;

	public GameObject playerEffect;
	public GameObject ScorePanel;
	float bigSize = 65f;

	void Start () {
		return;


		/*print (Network.player);
		print(Network.player.guid);*/

		//CmdSpawnScorePanel ();
		if (isLocalPlayer) {
			gameObject.name = "Local Player";
			CmdSetUpPlayer ();
			if (isServer) {
				id = 0;
				print ("We are the Host = " + id);
				//print (connectionToClient.connectionId);
				//SpawnPanel ();
				//NetworkServer.Spawn (ScorePanel);
				Invoke("BitOfLag",0.1f);	
			}
		} else {

			//gameObject.SetActive (false);
		}
		/*else if (id >= 0) {
			SpawnPanel ();
		} else {
			Invoke ("ReCheck", 0.5f);
		}*/


	}

	void ReCheck () {
		if (id >= 0) {
			SpawnPanel ();
		} else {
			Invoke ("ReCheck", 0.5f);
		}
	}
		

	[Command]
	void CmdSetUpPlayer () {
		RpcGetId (connectionToClient.connectionId);

	}

	[ClientRpc]
	void RpcGetId (int theId) {

		id = theId;
		print ("My Player Id = " + id);
		if (isLocalPlayer) {
			print ("Got Local Player Id = " + id);
		}
		//print (connectionToClient.connectionId);
		//SpawnPanel ();
		//NetworkServer.Spawn (ScorePanel);
		Invoke("BitOfLag",0.1f);
	}


	void SpawnPanel () {
		/*if (ScorePanel.GetComponent<ScorePanel> ().playerid != -1)
			return;*/

		print (id);
		RectTransform panelParent = GameObject.Find ("LeftPanel").GetComponent<RectTransform>();
		ScorePanel = (GameObject)Instantiate (ScorePanel, panelParent.position, panelParent.rotation);
		ScorePanel.GetComponent<RectTransform> ().parent = panelParent;
		ScorePanel.GetComponent<RectTransform> ().localScale = panelParent.localScale;

		if (isLocalPlayer) {
			ScorePanel.GetComponent<LayoutElement> ().minHeight = bigSize;
			ScorePanel.GetComponent<RectTransform> ().SetAsLastSibling ();
			GameObject.Find ("PowerUps").GetComponent<RectTransform>().SetAsFirstSibling();
			ScorePanel.gameObject.name = "Score Panel Main Player";
		} else {
			GameObject mainPanel = GameObject.Find ("Score Panel Main Player");
			if(mainPanel != null)
				mainPanel.GetComponent<RectTransform> ().SetAsLastSibling ();
			GameObject.Find ("PowerUps").GetComponent<RectTransform>().SetAsFirstSibling();
		}

		/*ScorePanel.GetComponent<ScorePanel> ().playerid = id;
		print (ScorePanel.GetComponent<ScorePanel> ().playerid);*/
	}
}