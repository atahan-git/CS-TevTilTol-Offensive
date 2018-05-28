using UnityEngine;
using System.Collections;

public class KapiScript : MonoBehaviour {

	public Transform rotateObject;

	Quaternion goToRotation;
	float speed = 500f;

	public float openDegree = 110f;
	float closeDegree = 0f;

	bool currentState = false;

	float threshold = 0.1f;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void OnMouseDown (){

		if (currentState) {
			goToRotation.eulerAngles = new Vector3 (0, closeDegree, 0);
			currentState = false;
		} else {
			goToRotation.eulerAngles = new Vector3 (0, openDegree, 0);
			currentState = true;
		}

	}


	void Update () {
		rotateObject.localRotation = Quaternion.RotateTowards (rotateObject.localRotation, goToRotation, speed * Time.deltaTime);
	}
		
}
