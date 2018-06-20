using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSelectionMenu : MonoBehaviour {

	public static TeamSelectionMenu s;

	// Use this for initialization
	void Start () {
		s = this;
		ActivateMenu (false);
	}

	public GameObject menuCam;
	public GameObject menu;


	public GameObject fillerCam;


	public void ActivateMenu (bool toggle){
		menuCam.SetActive (toggle);
		menu.SetActive (toggle);

		TeamMenu.SetActive (true);
		TypeMenu.SetActive (false);
	}

	public void DisableFillerCam (){
		fillerCam.SetActive (false);
	}


	public GameObject TeamMenu;
	public GameObject TypeMenu;

	int team = 0;
	public void SelectTeam (int teamId){
		team = teamId;
		TeamMenu.SetActive (false);
		TypeMenu.SetActive (true);
		foreach (ColorableMaterial mat in TypeMenu.GetComponentsInChildren<ColorableMaterial> ()) {
			mat.SetColor (teamId);
		}
	}

	public void Back (){
		TeamMenu.SetActive (true);
		TypeMenu.SetActive (false);
	}

	public void SelectType (int type){
		PlayerRelay.localRelay.TeamSelected (team,type);
		TypeMenu.SetActive (false);
	}
}
