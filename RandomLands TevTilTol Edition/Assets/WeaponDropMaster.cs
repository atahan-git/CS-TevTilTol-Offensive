using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class WeaponDropMaster : NetworkBehaviour {

	public static WeaponDropMaster s;

	public List<WeaponDropBoxController> myBoxes = new List<WeaponDropBoxController>();
	public List<WeaponDropBoxController> nonActiveList = new List<WeaponDropBoxController> ();


	Vector2 readyTime = new Vector2 (10f,15f);

	// Use this for initialization
	void Awake () {
		s = this;

	}

	void Start (){
		if (isServer) {
			Invoke ("GetReady", Random.Range (readyTime.x, readyTime.y));
		}
	}
	
	// Update is called once per frame
	void GetReady () {
		WeaponDropBoxController myWeap = null;

		myBoxes = myBoxes.Where(item => item != null).ToList();
		nonActiveList = myBoxes.Where (item => item.isReady == false).ToList();

		if (nonActiveList.Count > 0) {
			myWeap = nonActiveList [Random.Range (0, nonActiveList.Count)];
			myWeap.GetReady ();
			print ("getReady invoked: " + myWeap.gameObject.name);
		} else {
			print ("getReady invoked: null");
		}


		Invoke ("GetReady", Random.Range (readyTime.x, readyTime.y));
	}
}
