using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayPattern : MonoBehaviour {

	public int id = 0;

	public int loopStart = 6;

	public Transform[] myPoints;

	void Start (){
		myPoints = new Transform[transform.childCount];

		for (int i = 0; i < transform.childCount; i++) {
			myPoints [i] = transform.GetChild (0);
		}
	}

	public Vector2 GiveSprayPoint (int point){
		if (point < loopStart) {
			return GiveV2 (myPoints[point]);
		} else {
			return GiveV2 (myPoints [((point - loopStart) % (myPoints.Length - loopStart)) + loopStart]);
		}
	}

	Vector2 GiveV2 (Transform input){
		return new Vector2 (input.transform.localPosition.x, input.transform.localPosition.y);
	}
}
