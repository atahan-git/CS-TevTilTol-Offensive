using UnityEngine;
using System.Collections;

public class Kanit : MonoBehaviour {

	public float time = 0.17f;
	public Renderer myRen;

	// Use this for initialization
	void Start () {
		StartCoroutine (lel());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	IEnumerator lel () {
		while (true) {
			myRen.enabled = false;

			yield return new WaitForSeconds (1f);

			myRen.enabled = true;

			yield return new WaitForSeconds (time);

			myRen.enabled = false;
		}
	}
}
