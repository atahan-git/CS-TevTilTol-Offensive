using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GunPicker : NetworkBehaviour {

	GameObject myCam;

	public float pickGunDistance = 4f;

	Animator anim;
	Animator anim2;
	//GunController gcont;

	// Use this for initialization
	void Start () {
		if (myCam == null)
			myCam = GetComponent<PlayerRelay>().
				myPlayer.
				GetComponentInChildren<Camera>().
				gameObject;

		anim = GetComponent<PlayerRelay> ().myPlayer.GetComponentInChildren<Animator> ();
		anim2 = GetComponent<GunController> ().anim2;
		isPickedGun = HintScript.isPickedGun;
		//gcont = GetComponent<GunController> ();
	}

	bool isPickedGun;

    // Update is called once per frame
    void Update() {
		if (isLocalPlayer) {
			//shoot a ray when key e pressed
			if (Input.GetKeyDown (KeyCode.E)) {
				Ray r = new Ray (myCam.transform.position, myCam.transform.forward);
				RaycastHit hit;
				if (Physics.Raycast (r, out hit, pickGunDistance, 256)) {
					GunBuilder myGunBuilder = hit.collider.gameObject.GetComponentInParent<GunBuilder> (); //get the gun builder

					if (!myGunBuilder)
						return;

					if (!isPickedGun)
						HintScript.isPickedGun = true;

					/*float distance = Vector3.Distance(myCam.transform.position, myGunBuilder.transform.position);
                //if it exists and close enough
                if (distance < pickGunDistance)
                {*/
					GunBuilder myRealGunBuilder = GunController.myGunCont.gameObject.GetComponent<GunBuilder> (); //get our gun builder


					GunBuilder.Gun temp = myRealGunBuilder.myGun;
					myRealGunBuilder.myGun = myGunBuilder.myGun;

					CmdPickGun (temp, myGunBuilder.gameObject);
					myRealGunBuilder.BuildGun ();

					anim.SetTrigger ("isPicked");
					anim2.SetTrigger ("isPicked");

					//delete pickup
					//Destroy(myGunBuilder.gameObject);
					//}
				}
			}

			Ray r2 = new Ray (myCam.transform.position, myCam.transform.forward);
			RaycastHit hit2;
			if (Physics.Raycast (r2, out hit2, pickGunDistance, 256)) {
				Debug.DrawLine (myCam.transform.position, hit2.point);

				AppearOnCanPickUp pick = hit2.collider.gameObject.GetComponentInParent<AppearOnCanPickUp> ();

				if (!pick)
					return;

				pick.CancelInvoke ();
				pick.HighLight ();
            

			}
		}
	}		


	[Command]
	void CmdPickGun (GunBuilder.Gun _gun, GameObject GunBuildObj){
		GunBuilder myGunBuilder = GunBuildObj.GetComponent<GunBuilder> ();
		myGunBuilder.myGun = _gun;
		myGunBuilder.BuildGun ();
	}
}
