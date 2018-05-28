using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSelectPanelMaster : MonoBehaviour {

	public GameObject heroSPanel;

	// Use this for initialization
	void Start () {
		for(int i = 0; i < STORAGE_CharacterPrefabs.s.heroes.Length; i++){
			GameObject myPanel = (GameObject)Instantiate (heroSPanel, transform);
			myPanel.GetComponent<HeroSelectPanel> ().myId = i;
			myPanel.GetComponent<HeroSelectPanel> ().SetUp ();
		}
	}
}
