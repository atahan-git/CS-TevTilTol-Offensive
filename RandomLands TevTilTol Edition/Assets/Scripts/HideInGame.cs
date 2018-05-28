using UnityEngine;
using System.Collections;

public class HideInGame : MonoBehaviour {


	void Awake () {
		transform.GetComponent<Renderer>().enabled = false;
	}
}
