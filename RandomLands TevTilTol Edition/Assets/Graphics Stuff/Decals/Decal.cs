using UnityEngine;
using System.Collections;

public class Decal : MonoBehaviour {

	public Sprite[] decals;
	public SpriteRenderer renderer;

	// Use this for initialization
	void Start () {

		renderer.sprite = decals [Random.Range (0, decals.Length)];
		Invoke ("Disappear", 30f);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void Disappear (){

		Destroy (gameObject);
	}
}
