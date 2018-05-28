using UnityEngine;
using System.Collections;

public class RunAnimation : MonoBehaviour {

	public static float walkSpeed = -1f;

	public Animator anim;

	//public bool isOneTimeAnimation = true;
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();

	}

	// Update is called once per frame
	void Update () {


	}

	void RunAnim (int runSpeed){

		if (anim != null && this.enabled) {
			//1 = 1 in 0.1 seconds
			anim.SetFloat ("RunSpeed", (float)runSpeed / 12f);
			//print (anim.speed);
			//print ("thas called");
			//if (isOneTimeAnimation)
			//anim.Play ("iddleShoot");
			//else 
			anim.SetBool ("isRunnning", true);
		}
	}
	void StopRunAnim (){
		if (anim != null) {
			anim.SetBool ("isRunnning", false);
		}
	}
}
