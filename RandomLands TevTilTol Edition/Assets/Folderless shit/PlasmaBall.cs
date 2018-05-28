using UnityEngine;
using System.Collections;

public class PlasmaBall : MonoBehaviour {

	public float speed = 0.2f;

	public GameObject explosionEffect;
	public GameObject explosion2;

	// Use this for initialization
	void Start () {
		transform.parent = null;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate () {
		transform.position += transform.forward * speed;
	}

	void OnTriggerEnter (Collider col) {
		if (col.gameObject.GetComponentInParent<Hp>()) {
			col.gameObject.GetComponentInParent<Hp>().Damage (100000);

			Instantiate (explosionEffect, transform.position, transform.rotation);
			Instantiate (explosion2, transform.position, transform.rotation);
			Destroy (gameObject);
		}
	}
			
}
