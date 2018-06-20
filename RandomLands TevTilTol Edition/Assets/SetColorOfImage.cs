using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetColorOfImage : MonoBehaviour {

	public int myColor;
	Image myImg;

	// Use this for initialization
	void Start () {
		myImg = GetComponent<Image> ();
		myImg.color = PossibleColors.s.colors [myColor];

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
