using UnityEngine;
using System.Collections;

public class TopOfUFOTrigger : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter (Collider myCol){
		if (myCol.GetComponentInParent<Health> () != null || myCol.GetComponentInChildren<Health> () != null) {
			HintScript.isOnTop = true;
		}
	}
}
