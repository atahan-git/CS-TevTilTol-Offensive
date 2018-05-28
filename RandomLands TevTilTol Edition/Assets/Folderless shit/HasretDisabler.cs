using UnityEngine;
using System.Collections;

public class HasretDisabler : MonoBehaviour {

	public GameObject hasret;

	// Use this for initialization
	void Start () {
		GameObject[] tempShit = UnityEngine.SceneManagement.SceneManager.GetSceneByName("School Level").GetRootGameObjects ();

		foreach (GameObject gay in tempShit) {

			if (gay.name == "HASRET") {
				hasret = gay;
				return;
			}
		}
			
		this.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void DisableThatShittyBuilding (bool state) {
		hasret.SetActive (state);
	}

}
