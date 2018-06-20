using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossibleColors : MonoBehaviour {

	public static PossibleColors s;
	public Color[] colors = new Color[6];
	/* Red
	 * Green
	 * Blue
	 * Orange
	 * Black
	 * White
	 */

	// Use this for initialization
	void Awake () {
		s = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
