using UnityEngine;
using System.Collections;

public class AdvancedAlignToGround : MonoBehaviour {

	public float alignTime = 5f;

	public float groundOffset = 0f;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

		RaycastHit hit = new RaycastHit ();
		Ray myRay = new Ray ();
		int layerMask = 2048;

		/*myRay = new Ray (transform.parent.position + transform.parent.up * 5f, new Vector3(0,-1,0));
		if (Physics.Raycast (myRay, out hit, 100, layerMask)) {
			Debug.DrawLine (myRay.origin, hit.point, Color.blue);
			//Vector3 normal = hit.normal;
		}*/

		Quaternion rot = Quaternion.FromToRotation (transform.parent.up, hit.normal) * transform.parent.rotation;

		layerMask = 6144;

		myRay = new Ray (transform.parent.position + transform.parent.up * 1f, new Vector3(0,-1,0));
		if (Physics.Raycast (myRay, out hit, 100, layerMask)) {
			Debug.DrawLine (myRay.origin, hit.point, Color.red);
			//Vector3 normal = hit.normal;
		}

		if (Vector3.Distance (transform.parent.position, hit.point) < 2f) {
			transform.rotation = Quaternion.Slerp (transform.rotation, rot, alignTime * Time.deltaTime);
			transform.position = Vector3.Lerp (transform.position, hit.point + new Vector3 (0, groundOffset, 0), alignTime * Time.deltaTime);
		} else {
			transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.identity, alignTime * Time.deltaTime / 2f);
			transform.position = Vector3.Lerp (transform.position, transform.parent.position, alignTime * Time.deltaTime / 2f);
		}
	}
}
