using UnityEngine;
using System.Collections;

public class MenuCamController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<MovePathMaster> ().StartPath (0);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
