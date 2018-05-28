using UnityEngine;
using System.Collections;

public class GunDrop : MonoBehaviour {

	public int gunLevel = 1;
	public int gunRarity = 0;
	public bool isDefOpen = false;

	//amount of possible parts per level
	/*int[] body = {2,2,2};
	int[] barrel = {2,2,2};
	int[] scope = {2,2,2};
	int[] stock = {2,2,2};
	int[] grip = {2,2,2};*/

	void Start (){
		if (isDefOpen) {
			Invoke ("lel", 0.5f);
			GetComponent<Rigidbody> ().AddTorque (Random.Range(15,100), Random.Range(15,100), Random.Range(15,100));
		}
		

		//LeStart ();
	}

	void lel(){
		MakeGun (gunLevel, gunRarity);
	}
	
	public void MakeGun (int level, int rarity) {

		//print ("this activated");
		gunLevel = level;
		gunRarity = rarity;

		//get my component and gun
		GunBuilder myGunBuilder = GetComponent<GunBuilder> ();
		GunBuilder.Gun myGun = myGunBuilder.myGun;

		//gun builder will handle randomising
		myGun.gunLevel = gunLevel;
		myGun.gunRarity = gunRarity;
		myGunBuilder.RandomizeGunParts ();
		myGunBuilder.SetGunParts ();

		//randomly pick level and part
		/*myGun.bodyType = giveRandomLevelPart(body);
		myGun.barrelType = giveRandomLevelPart(barrel);
		myGun.scopeType = giveRandomLevelPart(scope);
		myGun.stockType = giveRandomLevelPart(stock);
		myGun.gripType = giveRandomLevelPart(grip);*/

		//enable colliders
		myGunBuilder.shouldCollide = true;

		//create gun
		myGunBuilder.enabled = true;

	}

	/*int giveRandomLevelPart (int[] partType){

		int thisLevel = Random.Range (0, gunLevel);
		return Random.Range (1 + ((thisLevel) * 10), body[thisLevel] + 1 + ((thisLevel) * 10) );

	}*/
	
	// Update is called once per frame
	//void OnMouseHover () {

	
	//}

	/*public Renderer rend;
	void LeStart() {
		rend = GetComponent<Renderer>();
	}
	void OnMouseEnter() {
		rend.material.color = Color.red;
	}
	void OnMouseOver() {
		rend.material.color -= new Color(0.1F, 0, 0) * Time.deltaTime;
	}
	void OnMouseExit() {
		rend.material.color = Color.white;
	}*/
}
