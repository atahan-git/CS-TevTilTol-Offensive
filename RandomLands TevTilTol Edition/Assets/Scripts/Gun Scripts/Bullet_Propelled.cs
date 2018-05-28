using UnityEngine;
using System.Collections;

public class Bullet_Propelled : MonoBehaviour {

	public int startForce = 2000;
	public int startTorque = -100;

	public int continuousForce = 200;
	public int upForce = 8;
	public float continuousTorque = 0.1f;

	Rigidbody rigid;
	// Use this for initialization
	void Start () {

		rigid = GetComponent<Rigidbody> ();

		rigid.AddRelativeForce (0, 0, startForce);
		rigid.AddRelativeTorque (startTorque, 0, 0);

	}
	
	// Update is called once per frame
	void FixedUpdate () {
		rigid.AddRelativeForce (0, upForce, continuousForce);
		rigid.AddRelativeTorque (continuousTorque, 0, 0);
	}
}
