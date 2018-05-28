using UnityEngine;
using System.Collections;

public class ShootAnimation : MonoBehaviour {

	public Animator anim;

	//public bool isOneTimeAnimation = true;
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();

		//InvokeRepeating ("ShootAnim", 0f, 0.1f);
	}
	
	// Update is called once per frame
	void Update () {

		//if (!anim)
			//return;

		//if(Input.GetMouseButtonUp(0))
			//anim.SetBool ("isShooting", false);
	
	}

	void ShootAnim (int fireRate){

		if (anim != null && this.enabled) {
			anim = GetComponent<Animator> ();

			if (!anim) {
				Debug.LogError ("Animator is missing!" + this);
				return;
			}
		}

		//1 = 1 in 0.1 seconds
		anim.SetFloat("ShootSpeed", (float)fireRate / 300f);
		//print (anim.speed);
		//print ("thas called");
		//if (isOneTimeAnimation)
			//anim.Play ("iddleShoot");
		//else 
		anim.SetBool ("isShooting", true);

	}
	void StopShootAnim (){
		if (!anim) {
			anim = GetComponent<Animator> ();

			if (!anim) {
				Debug.LogError ("Animator is missing!" + this);
				return;
			}
		}

		anim.SetBool ("isShooting", false);
	}
}
