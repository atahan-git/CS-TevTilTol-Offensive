using UnityEngine;
using System.Collections;

public class HintController : MonoBehaviour {

	public static HintController s;

	public GameObject[] hints;
	public Transform theParent;

	// Use this for initialization
	void Start () {
		s = this;
		SpawnHint (0);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void SpawnHint (int type){
		if (PlayerPrefs.GetInt ("HintsEnabled", 1) == 1) {
			GameObject myHint = (GameObject)Instantiate (hints [type], theParent);
			myHint.transform.localRotation = Quaternion.identity;
			myHint.transform.localScale = new Vector3 (1, 1, 1);
			myHint.transform.localPosition = Vector3.zero;
		}
	}
}
