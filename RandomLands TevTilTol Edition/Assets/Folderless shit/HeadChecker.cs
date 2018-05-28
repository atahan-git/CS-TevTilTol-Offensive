using UnityEngine;
using System.Collections;

public class HeadChecker : MonoBehaviour {

	ZomgasHeadCheck zc;
	RollergunCheck rc;
	RocketTankCheck tc;

	public float pickDistance = 8f;

	GameObject myCam;

	// Use this for initialization
	void Start () {
		if (myCam == null)
			myCam = Camera.main.gameObject;

		zc = GetComponent<ZomgasHeadCheck> ();
		rc = GetComponent<RollergunCheck> ();
		tc = GetComponent<RocketTankCheck> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(1))
		{
			Ray r = new Ray(myCam.transform.position, myCam.transform.forward);
			RaycastHit hit;
			if (Physics.Raycast(r, out hit, pickDistance))
			{

				HeadScript hs = hit.collider.gameObject.transform.root.gameObject.GetComponent<HeadScript> ();

				if (hs) {
					hs.DestroyOneself ();
					ScoreController.myScore.AddScore (50);

					switch (hs.type) {
					case 0:
						zc.curValue++;
						break;
					case 1:
						rc.curValue++;
						break;
					case 2:
						tc.curValue++;
						break;
					default:
						break;
					}
				}
			}
		}

	}
}
