using UnityEngine;
using System.Collections;
using EZObjectPools;
using EZEffects;
using UnityEngine.UI;

public class GunController : MonoBehaviour {

	public bool autoFire = false;
	public bool dontFire = false;

	public GunSharedValues val;

	public int gunLevel = 1;

	public float maxAccuracy = 0.1f; //default and maximum accuracy
	public float decreaseAccuracy = 0.2f; //accuracy decreased per bullet
	public float increaseAccuracy = -0.01f; //accuracy increased per tick
	public float leastAccuracy = 1f; //least possible accuracy
	public float curAccuracy = 0.5f; //current accuracy
		
	public enum ammoTypes{normal, heatSink};
    private ammoTypes _ammoType = ammoTypes.normal;
	public ammoTypes ammoType {
        get
        {
            return _ammoType;
        }
        set
        {
            _ammoType = value;
            ChangeAmmoType();
        }
    }

	public int ammoCapacity = 20;
	public int curAmmo = 20;
	public float reloadSeconds = 2f;
	public float secondsPerAmmo = 0.5f;

	public int damage = 1;
	public float fireRate = 600f; //bullets per minute BPM

	/*Transform barrelPoint;
	public GameObject myCam;
	LineRenderer myLine;
	public EffectMuzzleFlash MuzzleEffect;
	public AudioSource Audio;*/

	public RectTransform[] crosshairs;
	public float crosshairMultiplier = 25f;
	public float startPos =	12f;
	
	bool isShooting = false;
	bool isReloading = false;

	//public GameObject bulletHoleDecal;

	static public GunController myGunCont;

	public Text TextMaxAmmo;
	public Text TextCurAmmo;
	public Slider SlideAmmo;


	bool canFireAgain = true;
	
	
	public float damageMultiplier = 1f;
	public float ammoMultiplier = 1f;
	public float fireRateMultiplier = 1f;
	public float reloadTimeMultiplier = 1f;

	Animator anim;
	//public bool Inspect = false;
	// Use this for initialization
	void Start () {

		if(GetComponent<UnityEngine.Networking.NetworkIdentity>()){
			if(!GetComponent<UnityEngine.Networking.NetworkIdentity>().isLocalPlayer){
				this.enabled = false;
				return;
			}
		}

		if (val == null)
			val = GetComponent<GunSharedValues> ();
		
		switch (PlayerPrefs.GetInt ("Diff")) {
		case 0:
			damageMultiplier = 2.5f;
			ammoMultiplier = 2f;
			reloadTimeMultiplier = 0.5f;
			break;
		case 3:
			damageMultiplier = 0.75f;
			break;
		case 4:
			damageMultiplier = 0.85f;
			ammoMultiplier = 2.5f;
			fireRateMultiplier = 1.5f;
			break;
		default:
			damageMultiplier = 1f;
			break;
		}
		

		if (myGunCont != null)
			Debug.LogError ("there should only be one gun!");
		myGunCont = this;

		anim = GetComponent<PlayerRelay>().myPlayer.GetComponentInChildren<Animator> ();

		GunSceneReferences myRef = GetComponent<PlayerRelay> ().myPlayer.GetComponent<GunSceneReferences> ();
		crosshairs = myRef.crosshairs;
		TextCurAmmo = myRef.TextCurAmmo;
		TextMaxAmmo = myRef.TextMaxAmmo;
		SlideAmmo = myRef.SlideAmmo;
	}
	
	// Update is called once per frame
	void Update () {
		/*if (Input.GetKeyDown (KeyCode.X)) {
			PlayerPrefs.DeleteAll ();
		}*/


		if (dontFire) {
			CancelInvoke ("ShootRay");
			return;
		}

		val.damage = (int)(damage * damageMultiplier);
		//val.barrelPoint = barrelPoint;

		if(autoFire && canFireAgain){
			ShootRay ();
			isShooting = true;
		}

		if (!isShooting && Input.GetKeyDown (KeyCode.F)) {
			anim.SetBool ("Inspect", true);
		}

		if (Input.GetKeyUp (KeyCode.F)) {
			anim.SetBool ("Inspect", false);
		}

		if (Input.GetMouseButtonDown (0) && !autoFire && !isReloading && canFireAgain && !Input.GetKey(KeyCode.LeftShift)) {
			InvokeRepeating("ShootRay", 0f, 60f/(fireRate * fireRateMultiplier));
			isShooting = true;
			/*if (ammoType == ammoTypes.heatSink) {
				CancelInvoke ("AddAmmo");
				InvokeRepeating ("AddAmmo", secondsPerAmmo, secondsPerAmmo);
			}*/
			anim.SetBool ("Inspect", false);
			///InvokeRepeating("ReduceAccuracy", 0f, 0.1f);
		}
		if (curAmmo <= 0) {
			CancelInvoke ("ShootRay");
			//RemoveLine ();
			anim.BroadcastMessage("StopShootAnim");

			anim.SetBool ("Inspect", false);
			/*if (ammoType == ammoTypes.heatSink) {
				CancelInvoke ("AddAmmo");
				InvokeRepeating ("AddAmmo", secondsPerAmmo, secondsPerAmmo);
			}*/
		}

		if(Input.GetMouseButtonUp(0) && !autoFire && !isReloading || Input.GetKey(KeyCode.LeftShift)){
			isShooting = false;
			CancelInvoke("ShootRay");
			//RemoveLine();
			anim.BroadcastMessage("StopShootAnim");

			if(curAmmo <= 0 && !isReloading)
				StartReload();
			/*if (ammoType == ammoTypes.heatSink) {
				CancelInvoke ("AddAmmo");
				InvokeRepeating ("AddAmmo", secondsPerAmmo, secondsPerAmmo);
			}*/
		}

		if (Input.GetKeyDown (KeyCode.R) && !isReloading && curAmmo < ammoCapacity) {
			isShooting = false;
			CancelInvoke("ShootRay");
			//RemoveLine();
			StartReload();
			anim.BroadcastMessage("StopShootAnim");
			anim.SetBool ("Inspect", false);
		}

		if(!autoFire && !dontFire)
			UpdateHUD ();
	}

	void OnDisable () {
		CancelInvoke ("ShootRay");
	}

	void FireAgain(){
		canFireAgain = true;
	}

	void UpdateHUD (){
		
		TextMaxAmmo.text = ammoCapacity.ToString ();
		TextCurAmmo.text = curAmmo.ToString ();
		SlideAmmo.maxValue = ammoCapacity;
		SlideAmmo.value = curAmmo;
	
	}

	void FixedUpdate(){
		if (dontFire)
			return;
		if (isShooting || !canFireAgain) {
			curAccuracy += decreaseAccuracy;
		} else {
			curAccuracy += increaseAccuracy;
		}
		//print (curAccuracy);
		curAccuracy = Mathf.Clamp (curAccuracy, maxAccuracy, leastAccuracy);
		val.accuracy = curAccuracy;

		if(!autoFire && !dontFire)
		UpdateCrosshair ();
	}

	void UpdateCrosshair(){

		//print (Screen.height);

		float crosshairPosition = (((float)curAccuracy * (float)crosshairMultiplier)) * ((float)Screen.height/360f) + (float)startPos;
		//print (crosshairPosition);
		if (crosshairPosition < startPos)
			crosshairPosition = startPos;

		crosshairs [0].anchoredPosition = new Vector2 (0, crosshairPosition);
		crosshairs [1].anchoredPosition = new Vector2 (crosshairPosition, 0);
		crosshairs [2].anchoredPosition = new Vector2 (0, -crosshairPosition);
		crosshairs [3].anchoredPosition = new Vector2 (-crosshairPosition, 0);


	}

	//These 2 controls the spread of the cone
	//public float scaleLimit = 2.0f;  curAccuracy  
	public float z = 10f;

	void ShootRay() {

		canFireAgain = false;
		Invoke ("FireAgain", 60f / (float)(fireRate * fireRateMultiplier));
		//if (myLine == null)
		//return;


		BroadcastMessage("Shoot");


		curAmmo -= 1;
		anim.BroadcastMessage ("ShootAnim", (fireRate * fireRateMultiplier));
		//print ("broadcasted");
	}


	void StartReload (){
		if (ammoType != ammoTypes.normal)
			return;

		isReloading = true;
		Invoke ("FinishReload", reloadSeconds * reloadTimeMultiplier);
		anim.BroadcastMessage ("ReloadAnim", reloadSeconds * reloadTimeMultiplier);
		//audio.Play ();
		//print ("Started Reloding...");
	}

	void FinishReload (){
		curAmmo = ammoCapacity;
		isReloading = false;
		ScoreController.myScore.AddScore (-ammoCapacity / 4);
		//print ("Done reloading!");
	}

	void AddAmmo (){
		if (curAmmo < ammoCapacity) {
			curAmmo += 1;
			ScoreController.myScore.AddScore (-1);
		}
	}

    void ChangeAmmoType()
    {

        if(ammoType == ammoTypes.heatSink)
        {
            CancelInvoke("AddAmmo");
            InvokeRepeating("AddAmmo", secondsPerAmmo, secondsPerAmmo);
        }
        else
        {
            CancelInvoke("AddAmmo");
        }
    }
}
