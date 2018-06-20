using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchFoV : MonoBehaviour {

	public Camera source;

	Camera myCam;
	// Use this for initialization
	void Awake () {
		myCam = GetComponent<Camera> ();
		GetComponent<cakeslice.OutlineEffect> ().enabled = false;
		GetComponent<cakeslice.OutlineEffect> ().enabled = true;
	}
	
	// Update is called once per frame
	void Update () {
		myCam.fieldOfView = source.fieldOfView;
	}
}
