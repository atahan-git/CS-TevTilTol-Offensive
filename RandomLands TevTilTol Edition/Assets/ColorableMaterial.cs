using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorableMaterial : MonoBehaviour {


	public void SetColor (int i){
		GetComponent<Renderer> ().material.color = PossibleColors.s.colors [i];
	}
}
