using UnityEngine;
using System.Collections;

public class HintScript : MonoBehaviour {

	public static bool isPickedGun = false;
	public static bool isPickedHead = false;
	public static bool isOnTop = false;
	public static bool isGameBender = false;
	public static bool isShoot = false;
	public static bool skip = false;
	public static bool isSkipSpawn = false;

	public enum HintType{
		movement,
		shoot,
		reload,
		run,
		getGun,
		dontDie,
		getOnTop,
		killIt,
		getHead,
		gameBend,
		gameshoot,
		skip,
	}

	public HintType myType;

	// Use this for initialization
	void Start () {
	
	}

	float counter = 3f;
	
	// Update is called once per frame
	void Update () {

		switch (myType) {
		case HintType.movement:
			if (Input.GetKeyDown (KeyCode.A) || Input.GetKeyDown (KeyCode.D) || Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.W)) {
				DestroySelfnSpawnNext (0);
			}
			break;
		case HintType.shoot:
			if (Input.GetMouseButtonDown (0)) {
				DestroySelfnSpawnNext (1);
			}
			break;
		case HintType.reload:
			if (Input.GetKeyDown (KeyCode.R)) {
				DestroySelfnSpawnNext (2);
			}
			break;
		case HintType.run:
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftAlt)) {
				DestroySelfnSpawnNext (3);
			}
			break;
		case HintType.getGun:
			if (isPickedGun)
				DestroySelfnSpawnNext (4);
			break;
		case HintType.dontDie:
			if (counter < 0) {
				DestroySelf (5);
			}
			break;
		case HintType.getOnTop:
			if (isOnTop)
				DestroySelfnSpawnNext (6);
			break;
		case HintType.getHead:
			if (isPickedHead)
				DestroySelf (8);
			break;
		case HintType.gameBend:
			if (isGameBender)
				DestroySelfnSpawnNext (9);
			break;
		case HintType.gameshoot:
			if (isShoot)
				DestroySelf (10);
			break;
		case HintType.skip:
			if (skip)
				DestroySelf (11);
			break;
		}

		counter -= Time.deltaTime;
	}

	void DestroySelf (int type){
		PlayerPrefs.SetInt (type.ToString (), 1);
		Destroy (gameObject);
	}
	void DestroySelfnSpawnNext (int type){
		PlayerPrefs.SetInt (type.ToString (), 1);
		GetComponentInParent<HintController> ().SpawnHint (type + 1);
		Destroy (gameObject);
	}
}
