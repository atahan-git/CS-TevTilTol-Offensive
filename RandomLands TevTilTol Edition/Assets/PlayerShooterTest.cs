using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooterTest : MonoBehaviour {


	public int dmgval = 10;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.C)) {
			PlayerRelay.localRelay.GetComponent<Hp>().Damage (dmgval, -1,0, transform.position);
		}

		if (Input.GetKeyDown (KeyCode.X)) {
			StartCoroutine (Shoot ());
		}
	}


	IEnumerator Shoot (){
		int shootAmount = Random.Range (3, 5);
		float shootSpeed = Random.Range (600, 800);
		shootSpeed = shootSpeed / 60f;
		shootSpeed = 1f / shootSpeed;
		for (int i = 0; i < shootAmount; i++) {
			PlayerRelay.localRelay.GetComponent<Hp> ().Damage (dmgval, -1,0, transform.position);
			yield return new WaitForSeconds (shootSpeed);
		}
	}
}
