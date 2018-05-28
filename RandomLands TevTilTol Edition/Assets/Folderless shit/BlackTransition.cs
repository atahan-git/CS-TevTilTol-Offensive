using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BlackTransition : MonoBehaviour {

	public Image myimage;

	float alpha = 1f;

	float speed = 0.02f;

	// Use this for initialization
	void Start () {
	
	}

	public void Trans () {
		myimage.color = new Color (0, 0, 0, 1);
		alpha = 1f;
	}
	
	// Update is called once per frame
	void Update () {
		myimage.color = new Color (0, 0, 0, alpha);

		alpha -= speed;
		if (alpha > 1f)
			alpha = 1f;
	}
}
