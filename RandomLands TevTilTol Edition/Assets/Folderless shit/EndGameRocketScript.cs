using UnityEngine;
using System.Collections;

public class EndGameRocketScript : MonoBehaviour {

	public Transform saatKulesiTrans;

	public MonoBehaviour[] toBeEnabledS;
	public TrailRenderer traills;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.LookAt (saatKulesiTrans.position);

		if (Vector3.Distance (transform.position, saatKulesiTrans.transform.position) < 1) {
			Destroy (gameObject);
		}
	}

	public void Launch () {
		traills.enabled = true;
		foreach (MonoBehaviour mono in toBeEnabledS)
			mono.enabled = true;
	}

	void OnDestroy () {
		DeathController.s.Die ();
	}


}
