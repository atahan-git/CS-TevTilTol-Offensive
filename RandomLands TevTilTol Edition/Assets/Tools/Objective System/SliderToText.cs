using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SliderToText : MonoBehaviour {

	public Text text;
	public Slider slider;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		text.text = slider.value.ToString () + "/" + slider.maxValue.ToString ();
	
	}
}
