using UnityEngine;
using System.Collections;

public class FlyJets : MonoBehaviour {

	UnityEngine.AI.NavMeshAgent nav;
	public Animator anim;

	public ParticleSystem afterburner;

	// Use this for initialization
	void Start () {
		nav = GetComponentInParent<UnityEngine.AI.NavMeshAgent> ();
		CloseJets ();
	}

	public bool isOpen = false;

	// Update is called once per frame
	void Update () {

		if (isOpen) {
			if (!nav.isOnOffMeshLink)
				CloseJets ();
		} else {
			if (nav.isOnOffMeshLink)
				OpenJets ();
		}
	}


	void OpenJets(){
		isOpen = true;
		afterburner.Play ();
		anim.SetBool ("isOpen", true);
	}

	void CloseJets(){
		isOpen = false;
		afterburner.Stop ();
		anim.SetBool ("isOpen", false);
	}
}
