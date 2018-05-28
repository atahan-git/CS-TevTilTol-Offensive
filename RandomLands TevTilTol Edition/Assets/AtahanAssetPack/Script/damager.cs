using UnityEngine;
using System.Collections;

public class damager : MonoBehaviour {

	public int damage = 1;
    public float friendlyFireMultiplier = 0.5f;
    public bool canDamagePlayer = true;
	
	/*void OnCollisionStay (Collision ThingWeCollide) {
		DealDamage (ThingWeCollide.gameObject);
	}*/

	/*void OnCollisionEnter (Collision ThingWeCollide) {
		DealDamage (ThingWeCollide.gameObject);
	}*/
		
	/*void OnTriggerStay (Collider ThingWeCollide) {
		DealDamage (ThingWeCollide.gameObject);
	}*/
		
	void OnTriggerEnter (Collider ThingWeCollide) {
		DealDamage (ThingWeCollide);
	}

	void DealDamage (Collider myCol)
    {

        if (myCol.gameObject.GetComponent<Health>())
        {
            //print ("found helath component");
            Health dealtarget = myCol.gameObject.GetComponent<Health>();
            dealtarget.Damage(damage, transform);
        }

        Hp leDealTarget = null;

        if (myCol.GetComponent<Hp>())
        {
            leDealTarget = myCol.GetComponent<Hp>();
        }
        else if (myCol.GetComponentInChildren<Hp>())
        {
            leDealTarget = myCol.GetComponentInChildren<Hp>();
        }
        else if (myCol.GetComponentInParent<Hp>())
        {
            leDealTarget = myCol.GetComponentInParent<Hp>();
        }
        if (leDealTarget != null)
        {
            leDealTarget.Damage((int)((float)damage * friendlyFireMultiplier));
        }
    }

}
