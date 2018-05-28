using UnityEngine;
using System.Collections;

public class Fly : MonoBehaviour {

	public float doubleTapTime = 0.5f;
	float lastTime = 0f;

	bool flightToggle = false;

	public MonoBehaviour FPSC;

	public float speed = 0.1f;

	public Transform cam;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown (KeyCode.Space)) {
			if (lastTime > 0f) {

				if (flightToggle == false) {
					EnableFlight ();
					flightToggle = true;
				} else {
					DisableFlight ();
					flightToggle = false;
				}

			} else {
				lastTime = doubleTapTime;
			}
		}
		lastTime -= Time.deltaTime;

		lastTime = Mathf.Clamp (lastTime, 0f, doubleTapTime);


		if (flightToggle) {

			if (Input.GetKey (KeyCode.A))
				transform.Translate (Vector3.right 	* -speed * Time.deltaTime);
			if (Input.GetKey (KeyCode.D))
				transform.Translate (Vector3.right 	* speed * Time.deltaTime);
			if (Input.GetKey (KeyCode.W))
				transform.Translate (Vector3.forward 	* speed * Time.deltaTime);
			if (Input.GetKey (KeyCode.S))
				transform.Translate (Vector3.forward 	* -speed * Time.deltaTime);
			if (Input.GetKey (KeyCode.Space))
				transform.Translate (Vector3.up 		* speed * Time.deltaTime);
			if (Input.GetKey (KeyCode.LeftControl))
				transform.Translate (Vector3.up 		* -speed * Time.deltaTime);
			if (Input.GetKeyDown (KeyCode.LeftShift))
				speed = 50;
			if (Input.GetKeyUp (KeyCode.LeftShift))
				speed = 10;
		}
	}



	void EnableFlight (){

		GetComponent<CharacterController> ().enabled = false;
		//FPSC.enabled = false;
		GetComponent<Rigidbody> ().detectCollisions = false;
		GetComponent<Rigidbody> ().useGravity = false;
		//GetComponent<Rigidbody> ().velocity = Vector3.zero;

	}

	void DisableFlight (){

		GetComponent<CharacterController> ().enabled = true;
		//FPSC.enabled = true;
		GetComponent<Rigidbody> ().detectCollisions = true;
		GetComponent<Rigidbody> ().useGravity = true;

	}
}
