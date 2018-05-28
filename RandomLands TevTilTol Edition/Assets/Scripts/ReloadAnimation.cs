using UnityEngine;
using System.Collections;

public class ReloadAnimation : MonoBehaviour {

	public Animator anim;

	//public bool isOneTimeAnimation = true;
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		
		//InvokeRepeating ("ShootAnim", 0f, 0.1f);
	}
	
	// Update is called once per frame
	void Update () {
		
		if (!anim)
			return;
		
		//if(Input.GetMouseButtonUp(0))
		//anim.SetBool ("isShooting", false);
		
	}
	
	void ReloadAnim (float reloadTime){

		if (anim != null && this.enabled) {
			//1 = 1
			anim.SetFloat ("ReloadSpeed", 1f / (float)reloadTime);
			//print (anim.speed);
			//print ("this called");
			//if (isOneTimeAnimation)
			//anim.Play ("iddleShoot");
			//else 
			anim.SetTrigger ("isReloading");
		}
	}

}
