using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LobyPlayerController : NetworkBehaviour {

	public static LobyPlayerController localPlayer;

	public GameObject playerPanel;
	public GameObject prefabDataHandler;
	LobyPlayerPanel panelScript;
	NetworkLobbyPlayer manager;

	[SyncVar]
	public bool isReady;

	[SyncVar]
	public int id = -1;
	[SyncVar]
	public int playerSlot = -1;
	[SyncVar]
	public int heroType = 0;

	// Use this for initialization
	void Start () {

		manager = GetComponent<NetworkLobbyPlayer> ();

		GameObject panelParent = GameObject.Find ("PanelParent");
		playerPanel = (GameObject)Instantiate (playerPanel, panelParent.transform);
		playerPanel.transform.localScale = new Vector3 (0.8f, 0.8f, 0.8f);
		PanelPositionHandler.s.panels.Add (playerPanel);

		panelScript= playerPanel.GetComponent<LobyPlayerPanel> ();

		panelScript.playerid = id;
		panelScript.playerSlot = playerSlot;
		panelScript.playerState = manager.readyToBegin;
		panelScript.myPlayer = this;

		if (isLocalPlayer) {
			panelScript.isLocalPlayer = true;
			localPlayer = this;
			gameObject.name = "Local lobey player";
		}else 
			panelScript.isLocalPlayer = false;


		if (isServer && DataHandler.s == null) {
			GameObject _dataHandler = (GameObject)Instantiate (prefabDataHandler, Vector3.zero, Quaternion.identity);
			_dataHandler.GetComponent<DataHandler> ().SetUp ();
			NetworkServer.Spawn (_dataHandler);
		}

		if (isLocalPlayer)
			CmdSetUpPlayer ();
	}
		

	[Command]
	void CmdSetUpPlayer () {
		RpcGetId (connectionToClient.connectionId, DataHandler.s.playerSlots[connectionToClient.connectionId]);

	}

	[ClientRpc]
	void RpcGetId (int theId, int slot) {

		id = theId;
		playerSlot = slot;
		print ("My Player Id = " + id + " - " + gameObject.name);
		if (isLocalPlayer) {
			print ("Got Local Player Id = " + id + " - " + gameObject.name);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (SceneManager.GetActiveScene ().buildIndex == 0) {
			panelScript.playerid = id;
			panelScript.playerSlot = playerSlot;
			panelScript.playerState = isReady;
			panelScript.UpdateValues ();
			panelScript.heroType = heroType;
			manager.readyToBegin = isReady;
		}
	}

	public void OpenHeroMenu (){
		MenuMaster.s.OpenHeroSelectGUI (true);
	}

	public void ChangeHero(int type){
		MenuMaster.s.OpenHeroSelectGUI (false);
		heroType = type;
		heroType = Mathf.Clamp (heroType, 0, STORAGE_CharacterPrefabs.s.heroes.Length);
		DataHandler.s.heroIds [id] = heroType;
		CmdChangeHerotype (id, heroType);
	}

	[Command]
	public void CmdChangeHerotype (int id, int type){
		heroType = type;
		DataHandler.s.heroIds [id] = type;
		Update ();
		panelScript.UpdateValues ();
		RpcChangeHerotype (id, type);
	}

	[ClientRpc]
	public void RpcChangeHerotype (int id, int type){
		heroType = type;
		DataHandler.s.heroIds [id] = type;
		Update ();
		panelScript.UpdateValues ();
	}

	public void ChangeReadyState() {
		CmdChangeReadyState ();
	}

	[Command]
	public void CmdChangeReadyState () {
		if (isReady) {
			isReady = false;
		} else {
			isReady = true;
		}

		Update ();
		panelScript.UpdateValues ();
		GameObject.Find ("NetWorkManaGer").GetComponent<NetworkLobbyManager>().CheckReadyToBegin();
	}



	public void ChangeSlot(int slot){
		int oldSlot = playerSlot;
		playerSlot = slot;
		int toSwitch = DataHandler.s.playerSlots.IndexOf (playerSlot);

		DataHandler.s.playerSlots [id] = playerSlot;
		DataHandler.s.playerSlots [toSwitch] = oldSlot;
		CmdChangeSlot (id, playerSlot);
	}

	[Command]
	public void CmdChangeSlot (int id, int slot){
		int oldSlot = playerSlot;
		playerSlot = slot;
		int toSwitch = DataHandler.s.playerSlots.IndexOf (playerSlot);

		DataHandler.s.playerSlots [id] = playerSlot;
		DataHandler.s.playerSlots [toSwitch] = oldSlot;

		Update ();
		panelScript.UpdateValues ();
		RpcChangeSlot (id, slot);
	}

	[ClientRpc]
	public void RpcChangeSlot (int id, int slot){
		int oldSlot = playerSlot;
		playerSlot = slot;
		int toSwitch = DataHandler.s.playerSlots.IndexOf (playerSlot);

		DataHandler.s.playerSlots [id] = playerSlot;
		DataHandler.s.playerSlots [toSwitch] = oldSlot;

		Update ();
		panelScript.UpdateValues ();
	}
		




	public void ChangeBotHero (int slot, int amount){
		CmdChangeBotHero (slot, amount);
	}

	[Command]
	void CmdChangeBotHero (int slot, int amount){
		DataHandler.s.heroIds[slot] += amount;
		DataHandler.s.heroIds[slot] = Mathf.Clamp (DataHandler.s.heroIds[slot], 0, 3);
	}


	void OnDestroy (){
		Destroy (playerPanel);
	}
}
