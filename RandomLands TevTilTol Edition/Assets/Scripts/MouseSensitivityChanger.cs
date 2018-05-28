using UnityEngine;
using System.Collections;

public class MouseSensitivityChanger : MonoBehaviour {

    UnityStandardAssets.Characters.FirstPerson.FirstPersonController fpc;

	// Use this for initialization
	void Start () {
        fpc = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
	}

	float oldSens;
	
	// Update is called once per frame
	void Update () {

		float newSens = PlayerPrefs.GetFloat ("MouseSensitivity", 2);

		if (oldSens != newSens) {
			fpc.ChangeMouseSensitivity (newSens, newSens);
		}
	}
}
