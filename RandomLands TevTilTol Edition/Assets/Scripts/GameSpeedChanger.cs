using UnityEngine;
using System.Collections;

public class GameSpeedChanger : MonoBehaviour {

	public float speed = 0.5f;
	public static float monsterSpeedMult = 1f;

	// Use this for initialization
	void Awake () {
		switch (PlayerPrefs.GetInt ("Diff")) {
		case 0:
			monsterSpeedMult = 0.6f;
			break;
		case 4:
			monsterSpeedMult = 1.65f;
			break;
		default:
			monsterSpeedMult = 1f;
			break;
		}
	}

	void Start () {
		/*Time.timeScale = speed;
		Time.fixedDeltaTime = Time.timeScale * 0.02f;*/
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
