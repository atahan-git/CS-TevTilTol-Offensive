using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class RandomLevelLoader : MonoBehaviour {

	public static RandomLevelLoader s;

	public int levelId = 2;
	public int menuId = 3;

	public GameObject myPlayer;

	void Awake () {
		s = this;
	}

	void Start (){
		Invoke ("betterStart", 0.2f);
	}

	void betterStart () {
		if (SceneManager.sceneCount > 1) {
			//print ("More than one scene is already open, returning...");
			return;
		}
		print ("Non loaded menu detected, menu is loading");
		//SceneManager.LoadSceneAsync(levelId, LoadSceneMode.Additive);
		if (!isLoad2) {
			isLoad2 = true;
			SceneManager.LoadSceneAsync (menuId, LoadSceneMode.Additive);
		}
	}

	// Use this for initialization
	public void LoadScene () {
		if (!isLoad) {
			isLoad = true;
			SceneManager.LoadSceneAsync (levelId, LoadSceneMode.Additive);
		}
	}

	public void EngageExploration () {
		if(myPlayer != null)
		myPlayer.SetActive (true);

	}

	public bool isLoad = false;
	public bool isLoad2 = false;
	public bool isLoad3 = false;

	public void BackToMenu () {
		if (!isLoad3) {
			isLoad3 = true;
			SceneManager.LoadScene (0);
		}
	}


	bool gay = false;

	// Update is called once per frame
	void Update () {
		if (!gay) {
			gay = true;
			betterStart ();
		}
	}
}
