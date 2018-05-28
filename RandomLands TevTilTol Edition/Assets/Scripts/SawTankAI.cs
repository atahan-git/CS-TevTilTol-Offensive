using UnityEngine;
using System.Collections;

public class SawTankAI : MonoBehaviour {

	public int level = 1;
	public int rarity = 0;

	public int xp = 50;
	public int xpRange = 25;

	public float friendlyFireMultiplier = 1f;
	//public bool shouldDealDamage = false;
	public GameObject itemDrop;
	public float itemDropChance = 8;

	GameObject exp;

	public enum expTypes{small,normal,big};
	public expTypes myType = expTypes.normal;
	public Rigidbody kafa;
	Hp Hpin;
	GameObject player;       
	UnityEngine.AI.NavMeshAgent nav; 
	float distance;
	public float visionDist = 40;
	//bool idle = true;
	public float attackDist = 1f;
	public int damage = 20;
	public float attackSpeed = 1.2f;
	public float animSpeed = 1f;
	public bool isAttacking = false;

	public float radious = 5.0f;
	public float power = 50.0f;
	public float expDamage = 40f;
	public float maxRange = 8f;

	public GameObject killerPrefab;
	//public BoxCollider hitCollider;
	public Animator anim;
	AudioSource audio;

	// Use this for initialization
	void Start () {
		Hpin = GetComponent<Hp>();
		nav = GetComponent <UnityEngine.AI.NavMeshAgent>();
		audio = GetComponent<AudioSource> ();
		anim.speed = animSpeed;
		
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

	// Update is called once per frame
	void Update () {
		if (player == null)
			player = GameObject.FindGameObjectWithTag ("Player");
		if (player == null)
			return; //player does not exist
		distance = Vector3.Distance (transform.position, player.transform.position);

		//cant see the player
		if (distance > visionDist) {
			nav.enabled = false;
			anim.SetBool ("isMoving", false);
			CancelInvoke ("AttackAgain");

			//attack the player
		}else if(distance <= attackDist && !isAttacking){
			nav.enabled = false;
			anim.SetBool ("isMoving", false);
			Attack ();

			//pursue the player
		}else if(!isAttacking){
			nav.enabled = true;
			anim.SetBool ("isMoving", true);
			nav.SetDestination (player.transform.position);
			CancelInvoke ("AttackAgain");
		}


		//no hp = death
		if(Hpin.hpi <= 0){
			Explode();
		}

		//start "burning"
		/*if (distance < burnRange && !isBurning) {

			isBurning = true;
			Invoke ("Explode", 0.5f);
			GameObject myfire = (GameObject)Instantiate(fire, firePos.position, firePos.rotation);
			myfire.transform.parent = transform;

		}*/
	}

	void Attack (){

		anim.SetTrigger ("Attack");
		audio.pitch = Random.Range (0.9f, 1.1f);
		audio.Play ();
		InvokeRepeating ("AttackAgain", attackSpeed, attackSpeed);
	}
	void AttackAgain (){
		audio.pitch = Random.Range (0.9f, 1.1f);
		anim.SetTrigger ("Attack");
		audio.Play ();
	}

	void FinishAttack(){
		isAttacking = false;
	}

	void OnChildTriggerEnter (Collider myCol){
		//print ("on trigger");
		if(myCol.gameObject.GetComponent<Health>()){
			//print ("found helath component");
			Health dealtarget = myCol.gameObject.GetComponent <Health>();
			dealtarget.Damage (damage, transform);
		}

		Hp leDealTarget = null;

		if (myCol.GetComponent<Hp> ()) {
			leDealTarget = myCol.GetComponent<Hp> ();
		} else if (myCol.GetComponentInChildren<Hp> ()) {
			leDealTarget = myCol.GetComponentInChildren<Hp> ();
		} else if (myCol.GetComponentInParent<Hp> ()) {
			leDealTarget = myCol.GetComponentInParent<Hp> ();
		}
		if(leDealTarget != null){	
			leDealTarget.Damage ((int)((float)damage * friendlyFireMultiplier));
		}

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
		GameObject myExp = (GameObject)Instantiate(exp, transform.position, transform.rotation);
		//myExp.GetComponent<UnityStandardAssets.Effects.ParticleSystemMultiplier> ().multiplier = 0.5f;
		zomIns = (Rigidbody)Instantiate(kafa, transform.position + new Vector3(0,1,0), transform.rotation);
		zomIns.AddRelativeForce(0, 500, 0);
		zomIns.AddRelativeTorque(Random.Range(15,100), Random.Range(15,100), Random.Range(15,100));

		//item drop
		//int randomChance = (int)Random.Range(0, itemDropChance);
		//print(randomChance);
		if(/*randomChance == 0*/ LootCounter.ShouldDropLoot(level)){

			Debug.LogWarning("item dropped");
			GameObject myItem = (GameObject)Instantiate(itemDrop, transform.position + Vector3.up, transform.rotation);
			myItem.GetComponent<GunDrop>().MakeGun(level, rarity);

		}

		//give xp
		int xpToGive = xp + Random.Range (-xpRange, xpRange);
		XpGiver.giveXp (xpToGive);
		ScoreController.myScore.AddScore ((int)((float)xpToGive * Random.Range(1f, Mathf.Sqrt((float)level))));
		Destroy (gameObject);
	}
}


