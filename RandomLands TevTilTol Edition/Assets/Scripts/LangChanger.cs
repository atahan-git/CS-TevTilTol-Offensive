using UnityEngine;
using System.Collections;

public class LangChanger : MonoBehaviour {

	//0 eng ---- 1 tur

	public GameObject menuParent;
	static GameObject _menupar;
	public MenuController menuCont;

	// Use this for initialization
	void Awake () {
		if (PlayerPrefs.GetInt ("Lang", -1) == -1) {

			PlayerPrefs.SetInt ("Lang", 0);
		}

		_menupar = menuParent;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetLang (int lang){
		PlayerPrefs.SetInt ("Lang", lang);
		//menuCont.StartCoroutine ("LangChange");
		SetTextLang();
	}

	public static void SetTextLang () {

		_menupar.BroadcastMessage ("UpdateText");
	}
}
