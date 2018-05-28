using UnityEngine;
using System.Collections;

public class BackPack : MonoBehaviour {

	Hp Hpin;

	// Use this for initialization
	void Start () {
		Hpin = GetComponent<Hp> ();
	}
	
	// Update is called once per frame
	void Update () {
		if(Hpin.hpi <= 0){
			Destroy (gameObject);
		}
	}
}
