using UnityEngine;
using System.Collections;

public class zomgazAI : MonoBehaviour {

	public int level = 1;
	public int rarity = 0;

	public int xp = 50;
	public int xpRange = 25;

	public float friendlyFireMultiplier = 1f;
	public GameObject itemDrop;
	public float itemDropChance = 8;
	public GameObject fire;
	public Transform firePos;
	bool isBurning = false;

	GameObject exp;

	public enum expTypes{small,normal,big};
	public expTypes myType = expTypes.normal;

	public Rigidbody zomKafa;
	Hp Hpin;
	GameObject player;       
	UnityEngine.AI.NavMeshAgent nav; 
	float distance;
	public float visionDist = 40;
	//bool idle = true;
	public float radious = 30.0f;
	public float power = 1000.0f;
	public float damage = 400f;
	public float maxRange = 8f;
	public float burnRange = 4f;
	public float burnTime = 1.5f;

	public AudioSource aud;

	public GameObject killerPrefab;
	// Use this for initialization
	void Start () {
		Hpin = GetComponent<Hp>();
		nav = GetComponent <UnityEngine.AI.NavMeshAgent>();
		aud = GetComponent<AudioSource> ();
		
		float speedMultiplier = GameSpeedChanger.monsterSpeedMult;
		nav.speed = nav.speed * speedMultiplier;
		nav.acceleration = nav.acceleration * speedMultiplier;

		switch (myType) {
		case expTypes.small:
			exp = STORAGE_Explosions.s.smallExp;
			break;
		case expTypes.normal:
			exp = STORAGE_Explosions.s.normalExp;
			break;
		case expTypes.big:
			exp = STORAGE_Explosions.s.bigExp;
			break;
		}
		//CalculateLevelHpnDamage ();
	}

	//public bool offMeshLink = false;
	
	// Update is called once per frame
	void Update () {

		//offMeshLink = nav.isOnOffMeshLink;

		if(player == null)
			player = GameObject.FindGameObjectWithTag("Player");
		if (player == null)
			return; //player does not exist
		distance = Vector3.Distance(transform.position, player.transform.position);
		
		if(distance > visionDist){
			
			nav.enabled = false;
		}else{
			nav.enabled = true;
			nav.SetDestination (player.transform.position);
		}
		
		
		//no hp = death
		if(Hpin.hpi <= 0 && !isBurning){
			Burn ();
		}

		//start "burning"
		if (distance < burnRange && !isBurning) {
			Burn ();
		}
	}

	void Burn (){
		aud.Play ();
		isBurning = true;
		Invoke ("Explode", burnTime);
		GameObject myfire = (GameObject)Instantiate(fire, firePos.position, firePos.rotation);
		myfire.transform.parent = transform;
	}

	void Explode (){

		EnemyCounter.count--;

		//explosion
		Vector3 explosionPos = transform.position;
		Collider[] colliders = Physics.OverlapSphere (explosionPos, radious);
		foreach (Collider hit in colliders){
			if(hit != null && hit.GetComponent<Rigidbody>() || hit.GetComponentInParent<Rigidbody>()){
				
				if (hit.GetComponent<ExplosionAffection> ())
					hit.GetComponent<ExplosionAffection> ().GetEffected ();
				else if (hit.GetComponentInParent<ExplosionAffection>())
					hit.GetComponentInParent<ExplosionAffection> ().GetEffected ();


				if (hit.GetComponent<Rigidbody> ())
					hit.GetComponent<Rigidbody>().AddExplosionForce(power, explosionPos, radious, 3.0f);
				else if (hit.GetComponentInParent<Rigidbody>())
					hit.GetComponentInParent<Rigidbody>().AddExplosionForce(power, explosionPos, radious, 3.0f);
				
			}

			//deal damage
			if(distance < maxRange){
				if(hit.GetComponent<Health>()){
					Health dealtarget = hit.gameObject.GetComponent <Health>();
					dealtarget.Damage ((int)((float)damage * (1f - (distance / maxRange)) ), transform);
					if (dealtarget.health <= 0 && dealtarget.isAlive)
						Instantiate (killerPrefab, transform.position, transform.rotation);
				}
			}

			//deal damage to other enemies
			Hp leDealTarget = null;

			if (hit.GetComponent<Hp> ()) {
				leDealTarget = hit.GetComponent<Hp> ();
			} else if (hit.GetComponentInChildren<Hp> ()) {
				leDealTarget = hit.GetComponentInChildren<Hp> ();
			} else if (hit.GetComponentInParent<Hp> ()) {
				leDealTarget = hit.GetComponentInParent<Hp> ();
			}
			if(leDealTarget != null){	
				float leDistance = Vector3.Distance(transform.position, hit.gameObject.transform.position);
				leDealTarget.Damage ((int)(Mathf.Clamp(((float)damage * friendlyFireMultiplier * (1f - (leDistance / maxRange)) ), 0, Mathf.Infinity)));
			}
		}
		
		//other stuff
		Rigidbody zomIns;
		Instantiate(exp, transform.position, transform.rotation);
		zomIns = (Rigidbody)Instantiate(zomKafa, transform.position + new Vector3(0,1,0), transform.rotation);
		zomIns.AddRelativeForce(0, 100, 0);
		zomIns.AddRelativeTorque(Random.Range(15,100), Random.Range(15,100), Random.Range(15,100));
		
		//item drop
		//int randomChance = (int)Random.Range(0, itemDropChance);
		//print(randomChance);
		if(/*randomChance == 0*/ LootCounter.ShouldDropLoot(level)){
			
			//Debug.LogWarning("item dropped");
			GameObject myItem = (GameObject)Instantiate(itemDrop, transform.position + Vector3.up, transform.rotation);
			myItem.GetComponent<GunDrop>().MakeGun(level, rarity);
			
		}

		//give xp
		int xpToGive = xp + Random.Range (-xpRange, xpRange);
		XpGiver.giveXp (xpToGive);
		ScoreController.myScore.AddScore ((int)((float)xpToGive * Random.Range(1f, Mathf.Sqrt((float)level))));


		Destroy (gameObject);
	}

	/*void CalculateLevelHpnDamage(){

		//health
		Hpin.maxhp = (int)((level / 2f + 5f) * 10);
		Hpin.hpi = Hpin.maxhp;

		//damage
		damage = (2f * level / 3f + 15f) * 20;

	}*/
}


