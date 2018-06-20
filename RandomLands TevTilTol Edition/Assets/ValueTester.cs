using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueTester : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		float rndVal = Random.Range (-1.0f, 1.0f);
		//print (rndVal.ToString() + " - " +  Quaternion.LookRotation (new Vector3 (rndVal*0.5f,1f,0).normalized).ToString("F4"));

		transform.localPosition = new Vector3 (rndVal * 0.5f, 1f, 0).normalized;
		transform.localRotation = Quaternion.LookRotation (new Vector3 (rndVal * 0.5f, 1f, 0).normalized);
	}
}
