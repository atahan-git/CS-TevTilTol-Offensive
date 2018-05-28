using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ImageFade : MonoBehaviour {


	public float fadeTime = 3f;
	public float startTime = 1f;

	float val = 1f;
	bool shouldStart = false;

	public Image image;

	// Use this for initialization
	void Start () {
		Invoke ("should", startTime);
		Invoke ("selfDestruction", 5f);
	}
	
	// Update is called once per frame
	void Update () {

		if(shouldStart)
			val = Mathf.Lerp (val, 0, fadeTime * Time.deltaTime);

		image.color = new Color (1, 1, 1, val);
	
	}

	void should(){
		shouldStart = true;
	}

	void selfDestruction (){
		Destroy (gameObject);
	}
}
