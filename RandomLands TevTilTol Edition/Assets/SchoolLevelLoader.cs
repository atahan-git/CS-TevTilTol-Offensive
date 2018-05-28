using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SchoolLevelLoader : MonoBehaviour {

	public int schoolId = 1;

	void Start (){
		Invoke ("betterStart", 0.2f);
	}

	void betterStart () {
		if (SceneManager.sceneCount > 1) {
			//print ("More than one scene is already open, returning...");
			return;
		}
		print ("Non loaded school detected, school is loading");
		//SceneManager.LoadSceneAsync(levelId, LoadSceneMode.Additive);
		SceneManager.LoadScene (schoolId, LoadSceneMode.Additive);
	}

}
