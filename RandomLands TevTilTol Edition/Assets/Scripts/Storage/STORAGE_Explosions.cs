using UnityEngine;
using System.Collections;

public class STORAGE_Explosions : MonoBehaviour {

	public static STORAGE_Explosions s;

	public GameObject smallExp;
	public GameObject normalExp;
	public GameObject bigExp;
	public GameObject head;
	public GameObject rocketExp;

	// Use this for initialization
	void Awake () {
		s = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
