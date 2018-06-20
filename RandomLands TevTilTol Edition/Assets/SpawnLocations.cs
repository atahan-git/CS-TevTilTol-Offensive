using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnLocations : MonoBehaviour {

	public static SpawnLocations s;

	public Transform[] locations;

	// Use this for initialization
	void Start () {
		s = this;

		foreach (Renderer rend in GetComponentsInChildren<Renderer>()) {
			if (rend)
				rend.enabled = false;
		}

		locations = new Transform[transform.childCount];
		int i = 0;
		foreach(Transform chld in transform){
			chld.transform.position += Vector3.up * 1.1f;
			locations [i] = chld;
			i++;
		}
	}


	public Transform GetSpawnLocation (){
		return locations[Random.Range(0,locations.Length)];
	}
}
