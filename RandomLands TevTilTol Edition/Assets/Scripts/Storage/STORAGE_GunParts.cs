using UnityEngine;
using System.Collections;

public class STORAGE_GunParts : MonoBehaviour {

	public static STORAGE_GunParts s;

	public NestedGameObject[] body;
	public NestedGameObject[] barrel;
	public NestedGameObject[] magazine;
	public NestedGameObject[] scope;
	public NestedGameObject[] stock;


	// Use this for initialization
	void Awake() {
		s = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	[System.Serializable]
	public class NestedGameObject {
		public GameObject[] GunParts;
	}
}
