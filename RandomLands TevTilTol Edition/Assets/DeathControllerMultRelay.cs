using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathControllerMultRelay : MonoBehaviour {

	public static DeathControllerMultRelay s;

	public MonoBehaviour[] stuffToDisableOnDeath;

	// Use this for initialization
	void Start () {
		s = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	public void ToggleDeath(bool toggle){
		foreach (MonoBehaviour mono in stuffToDisableOnDeath) {
			if (mono != null) {
				mono.enabled = toggle;
			}
		}
	}
}
