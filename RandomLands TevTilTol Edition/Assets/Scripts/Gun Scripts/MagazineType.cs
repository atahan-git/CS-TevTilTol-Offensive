using UnityEngine;
using System.Collections;

public class MagazineType : MonoBehaviour {

	GunController myGunCont;
	public GunController.ammoTypes ammoType = GunController.ammoTypes.normal;

	[Header("If heatsink")]
	public float secondsPerAmmo = 0.5f;

	// Use this for initialization
	void Start () {

		myGunCont = GunController.myGunCont;
		//this.enabled = false;

	}

	// Update is called once per frame
	void Update () {

	}


	void SetStats (int ThePriority){
		if (ThePriority != 1)
			return;

		myGunCont = GunController.myGunCont;
		myGunCont.ammoType = ammoType;
		myGunCont.secondsPerAmmo = secondsPerAmmo;
		//print ("anan zaaa");
	}
}
