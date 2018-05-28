using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuMaster : MonoBehaviour {

	public static MenuMaster s;


	public Text textPlayerCount;

	[SerializeField]
	GameObject startGUI;
	[SerializeField]
	GameObject lobbyGUI;
	[SerializeField]
	GameObject heroSelectGUI;

	GameObject buttonUp;


	// Use this for initialization
	void Start () {
		s = this;

		//-------------------Warning!
		//----------------Cancer Ahead
		if (textPlayerCount == null) {
			textPlayerCount = GameObject.Find ("PlayerText").GetComponent<Text> ();
			startGUI = GameObject.Find ("Start GUI");
			lobbyGUI = GameObject.Find ("Lobby GUI");;
			heroSelectGUI = GameObject.Find ("Hero Select GUI");
			GameObject.Find ("ButtonUp").GetComponent<Button> ().onClick.AddListener (IncreasePlayerCount);
			GameObject.Find ("ButtonDown").GetComponent<Button> ().onClick.AddListener (DecreasePlayerCount);
			GameObject.Find ("Host").GetComponent<Button> ().onClick.AddListener (HostaGame);
			GameObject.Find ("Join").GetComponent<Button> ().onClick.AddListener (JoinaGame);
			GameObject.Find ("Back").GetComponent<Button> ().onClick.AddListener (ExitaGame);
		}
		lobbyGUI.SetActive (false);
		startGUI.SetActive (true);
		heroSelectGUI.SetActive (false);

		DecreasePlayerCount ();
	}

	//GUI Button calls
	public void IncreasePlayerCount () {
		LobyController.s.ChangePlayerCount (1);
	}

	public void DecreasePlayerCount () {
		LobyController.s.ChangePlayerCount (-1);
	}
		

	public void HostaGame () {
		LobyController.s.HostaGame ();
	}

	public void JoinaGame () {
		LobyController.s.JoinaGame ();
	}

	public void ExitaGame () {
		LobyController.s.ExitaGame ();
	}


	//Lobby Controller helpers
	public void OpenStartGUI (){
		//this transition will hopefully be more animated an stuff
		MenuMaster.s.startGUI.SetActive (true);
		MenuMaster.s.lobbyGUI.SetActive (false);
	}

	public void OpenLobbyGUI (){
		MenuMaster.s.startGUI.SetActive (false);
		MenuMaster.s.lobbyGUI.SetActive (true);
	}

	public void OpenHeroSelectGUI (bool state){
		MenuMaster.s.heroSelectGUI.SetActive (state);
	}
}
