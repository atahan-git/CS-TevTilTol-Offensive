using UnityEngine;
using System.Collections;

public class BasicAlignToGround : MonoBehaviour {

	public float sizeFront = 1f;
	public float sizeBack = -1f;
	Vector3 front;
	Vector3 back;

	Quaternion defRot;

	public float alignTime = 5f;

	// Use this for initialization
	void Start () {
		defRot = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {

		RaycastHit hit = new RaycastHit ();
		int layerMask = 2048;

		Ray myRay = new Ray (transform.parent.position + transform.parent.right * sizeFront + transform.parent.up * 5f, new Vector3(0,-1,0));

		if (Physics.Raycast (myRay, out hit, 100, layerMask)) {
			Debug.DrawLine (myRay.origin, hit.point);

			front = hit.point;
		}
		hit = new RaycastHit ();

		myRay = new Ray (transform.parent.position + transform.parent.right * sizeBack + transform.parent.up * 5f, new Vector3(0,-1,0));

		if (Physics.Raycast (myRay, out hit, 100, layerMask)) {
			Debug.DrawLine (myRay.origin, hit.point);

			back = hit.point;
		}

		myRay = new Ray (transform.parent.position + transform.parent.up * 5f, new Vector3(0,-1,0));
		if (Physics.Raycast (myRay, out hit, 100, layerMask)) {
			Debug.DrawLine (myRay.origin, hit.point, Color.blue);
			//Vector3 normal = hit.normal;
		}


		Quaternion rot = Quaternion.LookRotation (front - back);
		//rot *= Quaternion.Euler (0, 90, 0);

		layerMask = 6144;

		myRay = new Ray (transform.parent.position + transform.parent.up * 1f, new Vector3(0,-1,0));
		if (Physics.Raycast (myRay, out hit, 100, layerMask)) {
			Debug.DrawLine (myRay.origin, hit.point, Color.red);
			//Vector3 normal = hit.normal;
		}

		if (Vector3.Distance (transform.parent.position, hit.point) < 2f) {
			transform.rotation = Quaternion.Slerp (transform.rotation, rot, alignTime * Time.deltaTime);
			transform.position = Vector3.Lerp (transform.position, hit.point, alignTime * Time.deltaTime);
		} else {
			transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.identity, alignTime * Time.deltaTime / 2f);
			transform.position = Vector3.Lerp (transform.position, transform.parent.position, alignTime * Time.deltaTime / 2f);
		}
	}
}
