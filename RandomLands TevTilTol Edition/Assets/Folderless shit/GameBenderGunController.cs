using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameBenderGunController : MonoBehaviour {

	public GameObject prefab;

	public Text textDps;
	public Text textDps2;
	public Text textcurAmmo;
	public Text textmaxAmmo;
	public Slider sliderAmmo;

	int curAmmo = 5;

	Transform launchPos;
	public GameObject plasma;
	public GameObject reloadObj;
	GameObject _reload;
	Transform reloadPos;

	Animator anim;
	AudioSource aud;

	public AudioClip miss;
	public AudioClip reload;
	public AudioClip beep;

	public bool shootLock = false;

	Camera myCam;
	// Use this for initialization
	void Start () {

		GameObject myGun = (GameObject)Instantiate (prefab, transform.position, transform.rotation);
		myGun.transform.parent = transform;

		myCam = Camera.main;
		launchPos = GetComponentInChildren<ShootPos> ().transform;
		reloadPos = GetComponentInChildren<ReloadPos> ().transform;
		anim = myGun.GetComponentInChildren<Animator> ();
		aud = GetComponent<AudioSource> ();

		_reload = (GameObject)Instantiate (reloadObj, reloadPos.position, reloadPos.rotation);
		_reload.transform.parent = transform;
		aud.Stop ();
		aud.clip = reload;
		aud.Play ();
		shootLock = true;
		Invoke ("UnLock", 10f);

		textDps.text = "10 000 DPS";
		textDps2.text = "10 000 DPS";
		textmaxAmmo.text = "5";
		sliderAmmo.maxValue = 5;
		sliderAmmo.value = 5;
		textcurAmmo.text = "5";

		HintScript.isGameBender = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (myCam == null)
			return;

		if (Time.timeScale == 0)
			return;

		if (this.enabled) {
			if (Input.GetMouseButtonDown (0) && !shootLock && curAmmo > 0) {
				shootLock = true;
				Invoke ("UnLock", 10f);
				ShootGey ();
			}
		
			CheckShoot ();

		}
	}

	void CheckShoot () {

		if (!shootLock && curAmmo > 0) {
			Ray ray = new Ray (myCam.transform.position, myCam.transform.forward);
			int layerMask = 511;

			RaycastHit hit;

			if (Physics.Raycast (ray, out hit, Mathf.Infinity, layerMask)) {
				if (hit.transform.GetComponentInParent<Hp> ()) {
					if (hit.transform.GetComponentInParent<Hp> ().gameObject.name == "UFO") {
						GameBenderCrossHair.s.CanShoot (true);

					} else {
						GameBenderCrossHair.s.CanShoot (false);
					}
				} else {
					GameBenderCrossHair.s.CanShoot (false);
				}
			} else {
				GameBenderCrossHair.s.CanShoot (false);
			}

		}else{
			GameBenderCrossHair.s.CanShoot (false);
		}
	}

	void UnLock () {
		shootLock = false;
		aud.clip = beep;
		aud.Play ();
	}

	void Shoot (){
	}

	void ShootGey(){

		if (myCam == null)
			return;

		Ray ray = new Ray (myCam.transform.position, myCam.transform.forward);
		int layerMask = 511;

		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, Mathf.Infinity, layerMask)) {
			if (hit.transform.GetComponentInParent<Hp> ()) {
				if (hit.transform.GetComponentInParent<Hp> ().gameObject.name == "UFO") {
					Hit (hit.point);

				} else {
					Miss ();
				}
			} else {
				Miss ();
			}
		} else {
			Miss ();
		}
	}

	void Hit (Vector3 pos) {

		HintScript.isShoot = true;

		curAmmo--;
		anim.SetTrigger ("Shoot");

		GameObject shoote = (GameObject)Instantiate (plasma, launchPos.position, launchPos.rotation);
		shoote.transform.parent = transform;


		Destroy (_reload);

		if (curAmmo > 0) {
			aud.Stop ();
			aud.clip = reload;
			aud.Play ();


			_reload = (GameObject)Instantiate (reloadObj, reloadPos.position, reloadPos.rotation);
			_reload.transform.parent = transform;

		} else {
			Miss ();
		}

		textcurAmmo.text = curAmmo.ToString ();
		sliderAmmo.value = curAmmo;
	}

	void Miss () {

		CancelInvoke ("UnLock");
		shootLock = false;

		aud.Stop ();
		aud.clip = miss;
		aud.Play ();
	}
}
