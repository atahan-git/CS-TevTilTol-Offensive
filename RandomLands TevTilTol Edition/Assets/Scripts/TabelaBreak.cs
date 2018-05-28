using UnityEngine;
using System.Collections;

public class TabelaBreak : MonoBehaviour {

	Hp hp;

	// Use this for initialization
	void Start () {
		hp = GetComponent<Hp> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (hp.hpi <= 0) {
			BroadcastMessage ("Die");
			Destroy (gameObject);
		}
	}

}
