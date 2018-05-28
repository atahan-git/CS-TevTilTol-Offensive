using UnityEngine;
using System.Collections;

public class Impact_Explode : MonoBehaviour {

	Hp Hpin;

	public float noExplodeTime = 0.1f;

	public float friendlyFireMultiplier = 1f;

	public GameObject exp;
	//public Rigidbody kafa;

	public float radious = 5.0f;//explosive effect radious
	public float power = 50.0f;//explosive effect power
	public float expDamage = 40f;//explosion damage
	public float maxRange = 8f;//damage range

	//public GameObject killerPrefab;

	// Use this for initialization
	void Start () {
		exp = null;
		Hpin = GetComponent<Hp> ();
		Invoke ("EnableCollider", noExplodeTime);
		Invoke ("Explode", 10f);
		exp = STORAGE_Explosions.s.rocketExp;
	}

	void EnableCollider(){
		GetComponent<Collider> ().enabled = true;
	}

	// Update is called once per frame
	void Update () {

		/*if(Hpin.hpi <= 0){
			Explode();
		}*/


	}

	void OnCollisionEnter (Collision col){
		Explode ();
		/*if (col.gameObject.transform.root.gameObject.name == "Ufo & Walls" && V2_EnemySpawner.isRoundTen)
            col.gameObject.transform.root.gameObject.GetComponentInChildren<Hp>().Damage((int)expDamage * 10);*/
    }

	public bool rocketExploded = false;

	void Explode (){

		float distance = Vector3.Distance (GameObject.FindGameObjectWithTag ("Player").transform.position, transform.position);

		//explosion
		Vector3 explosionPos = transform.position;

        ArrayList objectsList = new ArrayList();

		Collider[] colliders = Physics.OverlapSphere (explosionPos, radious);
        foreach (Collider hit in colliders)
        {
            Transform rootTransform = hit.transform.root;
            if (!objectsList.Contains(rootTransform))
                objectsList.Add(rootTransform);
        }


        foreach (Transform hit in objectsList)
        {
            if (hit != null && hit.GetComponent<Rigidbody>() || hit.GetComponentInParent<Rigidbody>())
            {

                if (hit.GetComponent<ExplosionAffection>())
                    hit.GetComponent<ExplosionAffection>().GetEffected();
                else if (hit.GetComponentInParent<ExplosionAffection>())
                    hit.GetComponentInParent<ExplosionAffection>().GetEffected();


                if (hit.GetComponent<Rigidbody>())
                    hit.GetComponent<Rigidbody>().AddExplosionForce(power, explosionPos, radious, 3.0f);
                else if (hit.GetComponentInParent<Rigidbody>())
                    hit.GetComponentInParent<Rigidbody>().AddExplosionForce(power, explosionPos, radious, 3.0f);

            }

            //deal damage
            if (distance < maxRange)
            {
                if (hit.GetComponent<Health>())
                {
                    Health dealtarget = hit.gameObject.GetComponent<Health>();
                    dealtarget.Damage((int)((float)expDamage * (1f - (distance / maxRange))), transform);
                    //if (dealtarget.health <= 0 && dealtarget.isAlive)
                    //Instantiate (killerPrefab, transform.position, transform.rotation);
                }
            }

            //deal damage to other enemies
            Hp leDealTarget = null;

            if (hit.GetComponent<Hp>())
            {
                leDealTarget = hit.GetComponent<Hp>();
            }
            else if (hit.GetComponentInChildren<Hp>())
            {
                leDealTarget = hit.GetComponentInChildren<Hp>();
            }
            else if (hit.GetComponentInParent<Hp>())
            {
                leDealTarget = hit.GetComponentInParent<Hp>();
            }
            if (leDealTarget != null)
            {
                float leDistance = Vector3.Distance(transform.position, hit.gameObject.transform.position);
                float damage = ((float)expDamage * friendlyFireMultiplier * (1f - (leDistance / maxRange)));
                ExplosionDamageMultiplier expMult = leDealTarget.GetComponent<ExplosionDamageMultiplier>();
                //print(damage + " - " + (int)(Mathf.Clamp(damage, 0, expDamage) * 1f) + " - " + (int)(Mathf.Clamp(damage, 0, expDamage)) + " - " + leDealTarget.gameObject.name);
                if (expMult)
                    leDealTarget.Damage((int)(Mathf.Clamp(damage, 0, expDamage) * expMult.multAmmount));
                else
                    leDealTarget.Damage((int)(Mathf.Clamp(damage, 0, expDamage)));
                /*if (leDealTarget.gameObject.name == "UFO")
                    leDealTarget.Damage((int)(expDamage * 5));*/
            }
        }

		//other stuff
		//Rigidbody zomIns;
		GameObject myExp = (GameObject)Instantiate(exp, transform.position, transform.rotation);
		//myExp.GetComponent<UnityStandardAssets.Effects.ParticleSystemMultiplier> ().multiplier = 0.5f;
		//zomIns = (Rigidbody)Instantiate(kafa, transform.position + new Vector3(0,1,0), transform.rotation);
		//zomIns.AddRelativeForce(0, 500, 0);
		//zomIns.AddRelativeTorque(Random.Range(15,100), Random.Range(15,100), Random.Range(15,100));

		rocketExploded = true;
		Destroy (gameObject);
	}
}
