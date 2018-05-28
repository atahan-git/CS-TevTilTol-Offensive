using UnityEngine;
using System.Collections;

public class TriggerChild : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter (Collider myCol){
		SendMessageUpwards ("OnChildTriggerEnter", myCol, SendMessageOptions.DontRequireReceiver);
	}

	void OnTriggerStay (Collider myCol){
		SendMessageUpwards ("OnChildTriggerStay", myCol, SendMessageOptions.DontRequireReceiver);
	}

	void OnTriggerExit (Collider myCol){
		SendMessageUpwards ("OnChildTriggerExit", myCol, SendMessageOptions.DontRequireReceiver);
	}
}
