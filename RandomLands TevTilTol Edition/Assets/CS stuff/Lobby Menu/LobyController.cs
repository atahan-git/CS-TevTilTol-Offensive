using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobyController : NetworkBehaviour {

	public static LobyController s;
	NetworkManager manager;

	public int playerCount = 2;


	void Awake (){
		if (s != null && s != this) {
			Destroy (this.gameObject);
		} else {
			s = this;
			DontDestroyOnLoad (this.gameObject);
		}
	}

	// Use this for initialization
	void Start () {
		manager = GetComponent<NetworkManager> ();
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
		//if this is not the correct scene at start load the correct scene
		if (SceneManager.GetActiveScene().buildIndex != 0) {
			SceneManager.LoadScene (0);
		}
	}
		

	public void ChangePlayerCount (int amount){

		playerCount += amount;
		playerCount = Mathf.Clamp (playerCount, 1, 9);
		MenuMaster.s.textPlayerCount.text = playerCount.ToString ();

		if(manager == null)
			manager = GetComponent<NetworkManager> ();
	}

	public bool isHost = false;
	public bool isOnline = false;

	public void HostaGame () {

		MenuMaster.s.OpenLobbyGUI ();

		manager.StartHost ();
		isHost = true;
		isOnline = true;
	}

	public void JoinaGame () {

		MenuMaster.s.OpenLobbyGUI ();

		manager.StartClient ();
		isHost = false;
		isOnline = true;
	}

	public void ExitaGame () {
		if(DataHandler.s != null)
			Destroy (DataHandler.s.gameObject);

		MenuMaster.s.OpenStartGUI ();

		if (isHost) {
			manager.StopHost ();
		} else {
			manager.StopClient ();
		}
			
		//UnityEngine.SceneManagement.SceneManager.LoadScene (0);

		allBotLobyPanels = new List<GameObject> ();

		isOnline = false;
		connectedPlayers = -1;
		oldConnectedPlayers = -1;
	}

	//-----------------------------------------------------------------Game Begin Stuff

	public GameObject masterScripts;
	//public GameObject cheatsCanvas;
	void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode){
		print ("-*-*-*-*-*-* Scene: " + scene.name + " was loaded  *-*-*-*-*-*-");
		if(scene.buildIndex != 0){
			if (isHost) {
				print ("-*-*-*-*-*-* Spawned master scripts *-*-*-*-*-*-");
				GameObject myMasters = (GameObject)Instantiate (masterScripts);
				NetworkServer.Spawn (myMasters);
				/*GameObject myCheats = (GameObject)Instantiate (cheatsCanvas);
				NetworkServer.Spawn (myCheats);*/
			}
		}
	}



	//-----------------------------------------------------------------BOT STUFF

	int connectedPlayers = -1;
	int oldConnectedPlayers = -1;
	public GameObject botLobyPanel;
	public List<GameObject> allBotLobyPanels = new List<GameObject>();

	void Update (){

		if (manager.isNetworkActive && DataHandler.s != null) {
			//print ("botPanel Spawn task");

			if (isHost) {
				connectedPlayers = manager.numPlayers;
				DataHandler.s.playerCount = connectedPlayers;
			} else {
				connectedPlayers = DataHandler.s.playerCount;
			}



			if (connectedPlayers != oldConnectedPlayers) {

				if (allBotLobyPanels.Count == 0 && connectedPlayers != 9) {
					while (connectedPlayers + allBotLobyPanels.Count < 9) {
						GameObject extraPanel = (GameObject)Instantiate (botLobyPanel, transform.position, transform.rotation);
						allBotLobyPanels.Add (extraPanel);
					}
				}

				if (connectedPlayers > oldConnectedPlayers) {
					
					while (connectedPlayers + allBotLobyPanels.Count > 9) {

						int num = allBotLobyPanels.Count;
						GameObject toDestroy = allBotLobyPanels [num - 1];
						allBotLobyPanels.Remove (toDestroy);
						Destroy (toDestroy);

						allBotLobyPanels.TrimExcess ();
					}


				} else if (connectedPlayers < oldConnectedPlayers) {


					while (connectedPlayers + allBotLobyPanels.Count < 9) {
						GameObject extraPanel = (GameObject)Instantiate (botLobyPanel);
						allBotLobyPanels.Add (extraPanel);
					}

				}
			}
			oldConnectedPlayers = connectedPlayers;

			isOnline = true;
		} else if (isOnline && !manager.isNetworkActive) {
			ExitaGame ();
			isOnline = false;
			connectedPlayers = -1;
			oldConnectedPlayers = -1;
		}
	}
}
