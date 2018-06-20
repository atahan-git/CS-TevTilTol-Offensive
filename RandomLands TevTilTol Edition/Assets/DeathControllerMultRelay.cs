using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathControllerMultRelay : MonoBehaviour {

	public static DeathControllerMultRelay s;

	public MonoBehaviour[] stuffToDisableOnDeath;

	public GameObject DeathScreen;
	public Text timer;

	float time = 0;

	// Use this for initialization
	void Start () {
		s = this;
	}
	
	// Update is called once per frame
	public void Update () {
		timer.text = time.ToString ("F2") + "s";

		time -= Time.deltaTime;

		time = Mathf.Clamp (time, 0f, 1000f);
	}


	public void ToggleDeath(bool toggle, float deathTimer){
		time = deathTimer;
		foreach (MonoBehaviour mono in stuffToDisableOnDeath) {
			if (mono != null) {
				mono.enabled = toggle;
			}
		}

		DeathScreen.SetActive (toggle);
	}
}
