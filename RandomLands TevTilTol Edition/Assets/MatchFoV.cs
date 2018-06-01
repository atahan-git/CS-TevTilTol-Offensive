using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchFoV : MonoBehaviour {

	public Camera source;

	Camera myCam;
	// Use this for initialization
	void Start () {
		myCam = GetComponent<Camera> ();
	}
	
	// Update is called once per frame
	void Update () {
		myCam.fieldOfView = source.fieldOfView;
	}
}
