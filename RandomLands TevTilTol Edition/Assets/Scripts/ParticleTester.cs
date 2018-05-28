using UnityEngine;
using System.Collections;

public class ParticleTester : MonoBehaviour {

	public GameObject toTest;
	public float timeInBetween = 5f;

	// Use this for initialization
	void Start () {

		InvokeRepeating ("Spawn", 0f, timeInBetween);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void Spawn (){
		Instantiate (toTest, transform.position, transform.rotation);
	}
}
