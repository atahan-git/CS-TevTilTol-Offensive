using UnityEngine;
using System.Collections;

public class Pauser : MonoBehaviour {

	[Header("Pauser also controls Cursor States!")]

	public bool isPaused = false;

	public GameObject[] toBeDisabledG;
	public MonoBehaviour[] toBeDisabledS;

	public GameObject[] toBeEnabledG;
	public MonoBehaviour[] toBeEnabledS;

    DeathController dd;

	// Use this for initialization
	void Start () {
		if (!Application.isEditor) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
        dd = GetComponent<DeathController>();


		if (isPaused) {
			Pause ();
		}
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    // Update is called once per frame
    void Update() {

		if (dd) {
			if (Input.GetKeyDown (KeyCode.P) || Input.GetKeyDown (KeyCode.Escape) && !dd.areWeWon && !dd.areWeDead) {

				if (!isPaused) {
					Pause ();
				} else {
					UnPause ();
				}
			}
		} else {
			if (Input.GetKeyDown (KeyCode.P) || Input.GetKeyDown (KeyCode.Escape)) {

				if (!isPaused) {
					Pause ();
				} else {
					UnPause ();
				}
			}
		}
    }

	public void Pause () {
		isPaused = true;
		//Time.timeScale = 0;


		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;


		foreach (GameObject obj in toBeDisabledG)
			if(obj)
				obj.SetActive (false);
		foreach (MonoBehaviour mono in toBeDisabledS)
			if(mono)
				mono.enabled = false;

		foreach (GameObject obj in toBeEnabledG)
			if(obj)
				obj.SetActive (true);
		foreach (MonoBehaviour mono in toBeEnabledS)
			if(mono)
				mono.enabled = true;

		GunController.myGunCont.enabled = false;
	}

	public void UnPause () {
		isPaused = false;
		//Time.timeScale = 1;


		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;



		foreach (GameObject obj in toBeDisabledG)
			if (obj)
				obj.SetActive (true);
		foreach (MonoBehaviour mono in toBeDisabledS)
			if (mono)
				mono.enabled = true;

		foreach (GameObject obj in toBeEnabledG)
			if (obj)
				obj.SetActive (false);
		foreach (MonoBehaviour mono in toBeEnabledS)
			if (mono)
				mono.enabled = false;

		GunController.myGunCont.enabled = true;
	}
}
