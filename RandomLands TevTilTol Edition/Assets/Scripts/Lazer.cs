using UnityEngine;
using System.Collections;

public class Lazer : MonoBehaviour {

	public LineRenderer line;

	public int range = 200;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {


		Vector3[] positions = new Vector3[2];
		positions [0] = transform.position;
		positions [1] = transform.position + transform.forward * range;

		RaycastHit hit = new RaycastHit();

		if (Physics.Raycast (transform.position, transform.forward,out hit, range)) {

			positions [1] = hit.point;

		}

		line.SetPositions (positions);
	}
}
