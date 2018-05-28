using UnityEngine;
using System.Collections;
using EZObjectPools;
using EZEffects;
using UnityEngine.Networking;

public class Shoot_RaycastBullet : NetworkBehaviour {

	[HideInInspector]
	public GunSharedValues val;


	public GameObject bulletHoleDecal;
	LineRenderer myLine;
	//public EffectMuzzleFlash MuzzleEffect;
	public GameObject muzzleEffect;

	[HideInInspector]
	public float z = 10f;
	// Use this for initialization
	void Start () {



		if (val == null)
			val = GetComponent<GunSharedValues> ();
		if (val == null)
			val = GetComponentInParent<GunSharedValues> ();
		if (val == null)
			val = GetComponentInChildren<GunSharedValues> ();
	
	}
	
	// Update is called once per frame
	void Shoot () {

		//MuzzleEffect.SetupPool ();
		//  The Ray-hits will be in a circular area
		float randomRadius = Random.Range (0f, (float)val.accuracy);

		float randomAngle = Random.Range (0f, 2f * Mathf.PI);

		//Calculating the raycast direction
		Vector3 direction = new Vector3 (
			                    randomRadius * Mathf.Cos (randomAngle),
			                    randomRadius * Mathf.Sin (randomAngle),
			                    z
		                    );

		//Make the direction match the transform
		//It is like converting the Vector3.forward to transform.forward
		direction = val.myBulletSource.TransformDirection (direction.normalized);

		//Raycast and debug
		Ray r = new Ray (val.myBulletSource.position, direction);
		RaycastHit hit;        

		int layerMask = 1023;
		//print(layerMask);

		if (val.isPlayer)
			layerMask = 511;
		
		if (Physics.Raycast (r, out hit, Mathf.Infinity, layerMask)) {

			//deal damage
			//if (hit.collider.gameObject.tag == "Enemy") {

			//Debug.DrawLine (r.origin, hit.point);

			Hp hp = hit.collider.gameObject.GetComponent<Hp> ();
			if (hp == null)
				hp = hit.collider.gameObject.GetComponentInParent<Hp> ();
			if (hp == null)
				hp = hit.collider.gameObject.GetComponentInChildren<Hp> ();
			if (hp != null)
				CmdDamage (hp.gameObject, val.damage);

			//push the guy backwards
			/*if (hp != null) {
				/*Rigidbody rg = hp.GetComponent<Rigidbody> ();
				if (rg != null)
					rg.AddForceAtPosition (r.direction * 300f, hit.point);*/
			/*NavMeshAgent nav = hp.GetComponent<NavMeshAgent> ();
				Rigidbody rg = hp.GetComponent<Rigidbody> ();
				if (rg != null) {
					//nav.updatePosition = false;
					//nav.updateRotation = false;
					//rg.AddForceAtPosition (Vector3.up * 3000f, hit.point);
					/*nav.updatePosition = true;
					nav.updateRotation = true;
				}*
				//hp.transform.position += r.direction * ((float)val.damage / (float)hp.maxhp) * 1f;
				//print (r.direction * ((float)val.damage / (float)hp.maxhp) * 300f);
			}*/

			Health health = hit.collider.gameObject.GetComponent<Health> ();
			if (health == null)
				health = hit.collider.gameObject.GetComponentInParent<Health> ();
			if (health == null)
				health = hit.collider.gameObject.GetComponentInChildren<Health> ();
			if (health != null)
				CmdDamage (hp.gameObject, val.damage);

			//push the player backwards
			if (health != null) {
				//health.GetComponent<CharacterController> ().Move (r.direction * ((float)val.damage / (float)health.maxHealth) * 10f);
			}
					

			CmdShoot (val.barrelPoint.position, hit.point, hit.normal, hp == null); 

		} else {
			hit.point = (r.direction * 20) + r.origin;

			CmdShoot (val.barrelPoint.position, hit.point, hit.normal,false); 
		}

		//-------------------------
		//gfc
		Debug.DrawLine (val.barrelPoint.position, hit.point); 

		//MuzzleEffect.ShowMuzzleEffect(val.barrelPoint.transform, true, Audio);
		Instantiate (muzzleEffect, val.barrelPoint.position, val.barrelPoint.rotation);
		//----------------------------------------
	}

	void RemoveLine () {
		if (myLine == null)
			return;
		myLine.enabled = false;
	}


	[Command]
	void CmdDamage (GameObject tar, int dmgval) {
		if (tar.GetComponent<Hp> ())
			tar.GetComponent<Hp> ().Damage (dmgval, GetComponent<Hp>().mySide);
		else if (tar.GetComponent<Health> ())
			tar.GetComponent<Health> ().Damage (dmgval, transform);

	}

	[Command]
	void CmdShoot (Vector3 start, Vector3 end, Vector3 normal, bool isEffect) {
		ShowGfx (start, end, normal, isEffect);
		RpcShoot (start, end, normal, isEffect);
	}

	[ClientRpc]
	void RpcShoot (Vector3 start, Vector3 end, Vector3 normal, bool isEffect) {
		ShowGfx (start, end, normal, isEffect);
	}


	void ShowGfx (Vector3 start, Vector3 end, Vector3 normal, bool isEffect) {
		//print ("Showing Gfx " + gameObject.name);
		if (!myLine) {
			myLine = GetComponent<LineRenderer> ();
		}

		if (myLine) {
			myLine.enabled = true;
			myLine.SetPosition (0, start);
			myLine.SetPosition (1, end);
			Invoke ("RemoveLine", 0.1f);
		}

		//decals
		if (isEffect) {
			GameObject myDecal = (GameObject)Instantiate (bulletHoleDecal, end, Quaternion.FromToRotation (Vector3.up, normal));
		}
	}
}