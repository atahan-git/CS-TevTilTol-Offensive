using UnityEngine;
using System.Collections;

public class DynamicDoF : MonoBehaviour {

	public Transform focus;
	public Transform myCam;

	Vector3 leCorrectPos = new Vector3();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		RaycastHit leHit = new RaycastHit();
		float fakeZ = 0f;
		if (Physics.Raycast (myCam.position, myCam.forward, out leHit, 1000)) {

			fakeZ = focus.localPosition.z;
			focus.position = leHit.point;
			leCorrectPos = focus.position;
			focus.localPosition = new Vector3 (focus.localPosition.x, focus.localPosition.y, fakeZ);

            

		} else {
			
			fakeZ = focus.localPosition.z;
			focus.position = myCam.forward * 100;
			leCorrectPos = focus.position;
			focus.localPosition = new Vector3 (focus.localPosition.x, focus.localPosition.y, fakeZ);


		}

		focus.position = Vector3.Lerp (focus.position, leCorrectPos, 0.1f);
	
	}
}
