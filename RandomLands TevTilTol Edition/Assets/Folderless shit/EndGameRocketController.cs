using UnityEngine;
using System.Collections;

public class EndGameRocketController : MonoBehaviour {

	public static EndGameRocketController s;

	public GameObject rocket;
	public EndGameRocketScript sc;

	public Transform rocketStartPos;
	public Transform rocketEndPos;

	Vector3 goToPos;


	void Start () {
		s = this;
		rocket.SetActive (false);
		goToPos = rocket.transform.localPosition;
	}

	bool isEngaged = false;

	public void Engage(){

		rocket.SetActive (true);
		goToPos = Vector3.zero;
		isEngaged = true;
	}

	void Update () {

		if (isEngaged) {
			rocket.transform.localPosition = Vector3.Lerp (rocket.transform.localPosition, goToPos, 0.05f * Time.deltaTime);
		}

	}

	public void Launch(){
		sc.Launch ();
		isEngaged = false;
	}
}
