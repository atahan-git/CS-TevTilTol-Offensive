using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConvincingPhysicsSync : MonoBehaviour {

	public GameObject lerpHelper;

	float posLerp = 20f;
	float rotLerp = 20f;

	public float snapDist = 0.1f;

	public GameObject colliders;

	// Use this for initialization
	void Start () {
		lerpHelper = new GameObject ();
		lerpHelper.transform.position = transform.position;
		lerpHelper.transform.rotation = transform.rotation;
	}


	public void SetUpGunColliders (GameObject baseGun){
		int childs = colliders.transform.childCount;
		for (int i = childs - 1; i >= 0; i--)
		{
			GameObject.Destroy(colliders.transform.GetChild(i).gameObject);
		}

		GameObject colOnly = (GameObject)Instantiate (baseGun);
		colOnly.transform.position = baseGun.transform.position;
		colOnly.transform.rotation = baseGun.transform.rotation;
		colOnly.transform.parent = colliders.transform;

		foreach (Renderer rend in colOnly.GetComponentsInChildren<Renderer>()) {
			if (rend != null)
				rend.enabled = false;
		}

		foreach (Collider col in baseGun.GetComponentsInChildren<Collider>()) {
			if (col != null)
				col.enabled = false;
		}
	}

	
	// Update is called once per frame
	void Update () {

		if (Vector3.Distance (lerpHelper.transform.position,transform.position) > snapDist || Quaternion.Angle(lerpHelper.transform.rotation,transform.rotation) > snapDist) {
			lerpHelper.transform.position = Vector3.Lerp (lerpHelper.transform.position, transform.position, posLerp * Time.deltaTime);	
			lerpHelper.transform.rotation = Quaternion.Slerp (lerpHelper.transform.rotation, transform.rotation, rotLerp * Time.deltaTime);	
		} else {
			lerpHelper.transform.position = transform.position;
			lerpHelper.transform.rotation = transform.rotation;
		}

		foreach (Transform child in transform) {
			if (child.gameObject != colliders) {
				child.transform.position = lerpHelper.transform.position;
				child.transform.rotation = lerpHelper.transform.rotation;
			}
		}
	}
}
