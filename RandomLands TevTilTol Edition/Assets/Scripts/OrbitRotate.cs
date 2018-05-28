using UnityEngine;
using System.Collections;

public class OrbitRotate : MonoBehaviour {

    public float rotateSpeed = 1f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        transform.Rotate(new Vector3(0, rotateSpeed * Time.deltaTime, 0));
	
	}
}
