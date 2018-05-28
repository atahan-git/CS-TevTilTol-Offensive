using UnityEngine;
using System.Collections;

public class SpawnTARPAL : MonoBehaviour {

	public GameObject TARPAL;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void DoIt () {
		Instantiate (TARPAL, transform.position, transform.rotation);
	}
}
