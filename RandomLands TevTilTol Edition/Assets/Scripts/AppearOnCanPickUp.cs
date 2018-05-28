using UnityEngine;
using System.Collections;

public class AppearOnCanPickUp : MonoBehaviour {

    public GameObject highLightObject;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void HighLight()
    {
		if (this.enabled) {
			highLightObject.SetActive (true);
			Invoke ("unHighLight", 0.05f);
		}
    }

    void unHighLight()
    {
        highLightObject.SetActive(false);
    }
}
