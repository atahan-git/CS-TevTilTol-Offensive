using UnityEngine;
using System.Collections;

public class tupgaz : MonoBehaviour {

	public bool shouldDealDamage = false;

	public GameObject exp;
	float radious = 30.0f;
	float power = 1000.0f;
	float damage = 200f;
	Hp hp;

	void Start () {
		hp = GetComponent<Hp> ();
	}
	// Update is called once per frame
	void Update () {
	
		//no hp = death
		if(hp.hpi <= 0){
			//explosion
			Vector3 explosionPos = transform.position;
			Collider[] colliders = Physics.OverlapSphere (explosionPos, radious);
			foreach (Collider hit in colliders){
				if(hit.GetComponent<Rigidbody>()){
					hit.GetComponent<Rigidbody>().AddExplosionForce(power, explosionPos, radious, 3.0f);
				}
				//deal damage
				if(shouldDealDamage){
					float distance = Vector3.Distance(transform.position, hit.gameObject.transform.position);
					if(hit.GetComponent<Health>()){
						Health dealtarget = hit.gameObject.GetComponent <Health>();
						dealtarget.Damage ((int)(damage / distance*distance), transform);
					}
				}
			}

			Destroy(gameObject);
		}
	}
}