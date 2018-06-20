using UnityEngine;
using System.Collections;
using EZObjectPools;
using EZEffects;
using UnityEngine.Networking;

public class Shoot_RaycastBullet : NetworkBehaviour {

	[HideInInspector]
	public GunSharedValues val;


	public GameObject bulletHoleDecal;
	//public EffectMuzzleFlash MuzzleEffect;
	public GameObject muzzleEffect;
	public GameObject muzzleEffectLazer;

	public LineRenderer[] playerLineRenderers = new LineRenderer[1];
	public LineRenderer[] enemyLineRenderers = new LineRenderer[1];

	public bool isLazer;
	
	public int rand;
	public int[][][] pattern;
	public int patternSize;
	public int sprayCount;
	public int sprayTimer;
	public int sprayInterval = 100;
	
	[HideInInspector]
	public float z = 10f;
	// Use this for initialization
	void Start () {
		
		sprayTimer = 0;
		rand = (int) Random.range(0, 2);
		patternSize = pattern[rand].size;
		sprayCount = 0;
		
		if (val == null)
			val = GetComponent<GunSharedValues> ();
		if (val == null)
			val = GetComponentInParent<GunSharedValues> ();
		if (val == null)
			val = GetComponentInChildren<GunSharedValues> ();
	
	}
	
	void FixedUpdate(){
		if(sprayTimer == 0){
			sprayCount = 0;
		}else{
			sprayTimer--;
		}
	}
	
	// Update is called once per frame
	void Shoot () {
		sprayTimer = sprayInterval;
		//MuzzleEffect.SetupPool ();
		//  The Ray-hits will be in a circular area
		float randomRadius = Random.Range (0f, (float)val.accuracy);

		float randomAngle = Random.Range (0f, 2f * Mathf.PI);

		//Calculating the raycast direction
		Vector3 direction = new Vector3 (
			                    (randomRadius * Mathf.Cos (randomAngle) + pattern[rand][sprayCount % patternSize][0]) / 2,
			                    (randomRadius * Mathf.Sin (randomAngle) + pattern[rand][sprayCount % patternSize][1]) / 2,
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
				CmdDamage (health.gameObject, val.damage);

			//push the player backwards
			if (health != null) {
				//health.GetComponent<CharacterController> ().Move (r.direction * ((float)val.damage / (float)health.maxHealth) * 10f);
			}
			GameObject target = hit.collider.gameObject;		
			if (target.GetComponentInParent <NetworkIdentity> () != null && !target.GetComponent<GunDrop>()) {
				CmdShoot2 (hit.point, hit.normal, target.GetComponentInParent<NetworkIdentity>().gameObject); 
			} else {
				CmdShoot (hit.point, hit.normal, (hit.collider.gameObject.GetComponent<GunDrop> () == null));
			}

		} else {
			hit.point = (r.direction * 20) + r.origin;

			CmdShoot (hit.point, hit.normal, false); 
		}

		//-------------------------
		//gfc
		Debug.DrawLine (val.barrelPoint.position, hit.point); 

		//MuzzleEffect.ShowMuzzleEffect(val.barrelPoint.transform, true, Audio);
		//----------------------------------------
	}

	void RemoveLine () {
		foreach (LineRenderer rend in playerLineRenderers) {
			if (rend)
				rend.enabled = false;
		}
		foreach (LineRenderer rend in enemyLineRenderers) {
			if (rend)
				rend.enabled = false;
		}
	}


	[Command]
	void CmdDamage (GameObject tar, int dmgval) {
		if (tar.GetComponent<Hp> ())
			tar.GetComponent<Hp> ().Damage (dmgval, GetComponent<Hp> ().mySide);
		else if (tar.GetComponent<Health> ())
			tar.GetComponent<Health> ().Damage (dmgval, transform);

	}

	[Command]
	void CmdShoot (Vector3 end, Vector3 normal, bool isEffect) {
		//ShowGfx (end, normal, isEffect, null);
		RpcShoot (end, normal, isEffect);
	}

	[Command]
	void CmdShoot2 (Vector3 end, Vector3 normal, GameObject target) {
		//ShowGfx (end, normal, true, target);
		RpcShoot2 (end, normal, target);
	}

	[ClientRpc]
	void RpcShoot (Vector3 end, Vector3 normal, bool isEffect) {
		ShowGfx (end, normal, isEffect, null);
	}

	[ClientRpc]
	void RpcShoot2 (Vector3 end, Vector3 normal, GameObject target) {
		ShowGfx (end, normal, true, target);
	}


	void ShowGfx (Vector3 end, Vector3 normal, bool isEffect, GameObject target) {
		
		if (isLazer)
			Instantiate (muzzleEffectLazer, val.barrelPoint.position, val.barrelPoint.rotation);
		else
			Instantiate (muzzleEffect, val.barrelPoint.position, val.barrelPoint.rotation);

		//print ("Showing Gfx " + gameObject.name);
		LineRenderer myLine;
		int i = isLazer ? 1 : 0;
		if (isLocalPlayer) {
			myLine = playerLineRenderers [i];
		} else {
			myLine = enemyLineRenderers [i];
		}

		if (myLine) {
			myLine.enabled = true;
			myLine.SetPosition (0, val.barrelPoint.position);
			myLine.SetPosition (1, end);
			Invoke ("RemoveLine", 0.1f);
		}

		if (target != null)
			print (isEffect.ToString() + " Decal Target = " + target.transform.root.name);
		else
			print (isEffect.ToString() + " Decal Target is Null");

		//decals
		if (isEffect) {
			GameObject myDecal = (GameObject)Instantiate (bulletHoleDecal, end, Quaternion.FromToRotation (Vector3.up, normal));
			if (target != null) {
				myDecal.transform.parent = target.transform;
				print ("Decal Parent Set: " + myDecal.transform.parent.name);
			}
		}
	}
}