using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableCostlyEffects : MonoBehaviour {

	public static List<DisableCostlyEffects> allEffects = new List<DisableCostlyEffects>();

	public MonoBehaviour[] myEffects = new MonoBehaviour[2];

	// Use this for initialization
	void Start () {
		allEffects.Add (this);
		SetState (PlayerPrefs.GetInt ("Quality", 5));
	}

	public void SetState (int level){
		bool state = false;
		if (level > 0) {
			state = true;
		}

		foreach (MonoBehaviour mono in myEffects) {
			mono.enabled = state;
		}
	}
}
