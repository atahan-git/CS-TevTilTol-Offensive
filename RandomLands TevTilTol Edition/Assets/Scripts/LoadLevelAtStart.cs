using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadLevelAtStart : MonoBehaviour {

	public int levelid = 1;

	bool gey = false;

	// Use this for initialization
	void Start(){
		Invoke("LoadNow", 0.5f);

		//LoadNow();
		//print ("obür amnu kooo");
	}
	
	// Update is called once per frame
	void Update () {
		if (!gey) {
			gey = true;
			LoadNow ();
		}
	}
	bool isLoading =false;

    void LoadNow()
    {
		if (!isLoading) {
			//print ("geyyilk");
			SceneManager.LoadSceneAsync (levelid);
			//print ("AMıNA KODUMUN YUKLEN");
			isLoading = true;
		}
    }
}
