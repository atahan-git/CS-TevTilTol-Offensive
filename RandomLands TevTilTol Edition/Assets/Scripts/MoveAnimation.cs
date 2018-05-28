using UnityEngine;
using System.Collections;

public class MoveAnimation : MonoBehaviour {

    Vector3 lastFramePos;

    Animator anim;

	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
        lastFramePos = transform.position;
		anim.SetBool("isMoving", true);
	}
	
	// Update is called once per frame
	void Update () {
	    /*if(Vector3.Distance(lastFramePos, transform.position) > 0)
        {
            anim.SetBool("isMoving", true);
            anim.speed = Vector3.Distance(lastFramePos, transform.position);
        }
        else
        {
            //anim.SetBool("isMoving", false);
        }*/
		anim.speed = Vector3.Distance(lastFramePos, transform.position);
        lastFramePos = transform.position;
	}
}
