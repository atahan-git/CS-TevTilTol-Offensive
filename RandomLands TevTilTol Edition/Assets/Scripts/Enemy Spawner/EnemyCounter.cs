using UnityEngine;
using System.Collections;

public class EnemyCounter : MonoBehaviour {

	public static int count = 0;

	// Use this for initialization
	void Start () {
		//print (count);
		count = 0;
		lastCount = 0;
		//print (count);
		InvokeRepeating("ReduceBulletCounter",1f,10f);
	}

	int lastCount = 0;
	// Update is called once per frame
	void Update () {
		/*if (lastCount != count)
			print (count);*/

		if (count < 0)
			count = 0;

		lastCount = count;

		//Mathf.Clamp (count, 0, Mathf.Infinity);
	}

	void ReduceBulletCounter (){

		AiGunScript.counter = 0;
	}
}
