using UnityEngine;
using System.Collections;

public class HeadScript : MonoBehaviour {

	public static int headCount = 0;

	public int type = -1;

	public GameObject glowObj;
	public AppearOnCanPickUp appear;

	// Use this for initialization
	void Start () {
		headCount++;
		float leAm = 3;
		if (headCount > 50 && (type != 2 || type != 1)) {
			leAm = 2;
		}

		if (type == -1) {
			leAm = 0.5f;
		}
		Invoke ("DestroyOneself", leAm * 60);

		appear = GetComponent<AppearOnCanPickUp> ();

		GlowOff ();

		switch (type) {
		case 0:
			if (ZomgasHeadCheck.leActive)
				GlowOn ();
			else
				GlowOff ();

			ZomgasHeadCheck.glowCallON.AddListener (GlowOn);
			ZomgasHeadCheck.glowCallOFF.AddListener (GlowOff);
			break;
		case 1:
			if (RollergunCheck.leActive)
				GlowOn ();
			else
				GlowOff ();

			RollergunCheck.glowCallON.AddListener (GlowOn);
			RollergunCheck.glowCallOFF.AddListener (GlowOff);
			break;
		case 2:
			if (RocketTankCheck.leActive)
				GlowOn ();
			else
				GlowOff ();

			RocketTankCheck.glowCallON.AddListener (GlowOn);
			RocketTankCheck.glowCallOFF.AddListener (GlowOff);
			break;
		default:
			break;
		}
	}
	
	// Update is called once per frame
	void Update () {
		

	
	}

	void GlowOn () {
		if (glowObj) {
			glowObj.SetActive (true);
			appear.enabled = true;
		}

	}

	void GlowOff () {
		if (glowObj) {
			glowObj.SetActive (false);
			appear.enabled = false;
		}
	}

	public void DestroyOneself(){
		headCount--;

		Instantiate (STORAGE_Explosions.s.head, transform.position, Quaternion.identity);

		Destroy (gameObject);
	}
}
