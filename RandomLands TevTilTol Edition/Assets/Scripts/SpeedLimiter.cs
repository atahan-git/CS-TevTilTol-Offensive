using UnityEngine;
using System.Collections;

public class SpeedLimiter : MonoBehaviour {

	public float maxSpeed = 10f;

	Rigidbody rigid;

	// Use this for initialization
	void Start () {
		rigid = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		if (rigid.velocity.magnitude > maxSpeed) {

			rigid.velocity = rigid.velocity.normalized * maxSpeed;
		}


	}

}
