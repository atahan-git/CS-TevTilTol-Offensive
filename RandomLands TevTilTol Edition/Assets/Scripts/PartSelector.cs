using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PartSelector : MonoBehaviour {

	public enum PartTypes{Body, Barrel, Scope, Stock, Magazine};
	public PartTypes myType = PartTypes.Body;

	public int curLevel = 0;
	public int curType = 0;

	public Text partLevel;
	public Text partType;

	GunBuilder builder;

	// Use this for initialization
	void Start () {

		//partName.text = myType.ToString ();
		partLevel.text = curLevel.ToString ();
		partType.text = curType.ToString ();

		builder = GameObject.Find ("Gun").GetComponent<GunBuilder> ();
		//ChangeType (0);

	}
	
	// Update is called once per frame
	void Update () {

		switch (myType){
		case PartTypes.Body:
			curType = builder.myGun.bodyType;
			curLevel = builder.myGun.bodyLevel;
			break;
		case PartTypes.Barrel:
			curType = builder.myGun.barrelType;
			curLevel = builder.myGun.barrelLevel;
			break;
		case PartTypes.Scope:
			curType = builder.myGun.scopeType;
			curLevel = builder.myGun.scopeLevel;
			break;
		case PartTypes.Stock:
			curType = builder.myGun.stockType;
			curLevel = builder.myGun.stockLevel;
			break;
		case PartTypes.Magazine:
			curType = builder.myGun.magazineType;
			curLevel = builder.myGun.magazineLevel;
			break;
		}
		partType.text = curType.ToString ();
		partLevel.text = curLevel.ToString ();
	}

	public void ChangeType (int amount){
		//print ("valchange");
		//return;
		curType += amount;

		switch (myType){
		case PartTypes.Body:
			builder.myGun.bodyType = curType;
			break;
		case PartTypes.Barrel:
			builder.myGun.barrelType = curType;
			break;
		case PartTypes.Scope:
			builder.myGun.scopeType = curType;
			break;
		case PartTypes.Stock:
			builder.myGun.stockType = curType;
			break;
		case PartTypes.Magazine:
			builder.myGun.magazineType = curType;
			break;
		}
		//builder.BuildGun ();
		partLevel.text = curLevel.ToString ();
		partType.text = curType.ToString ();
		builder.StartCoroutine("BuildGun");
	}

	public void ChangeLevel (int amount){
		//print ("valchange");
		//return;
		curLevel += amount;

		switch (myType){
		case PartTypes.Body:
			builder.myGun.bodyLevel = curLevel;
			break;
		case PartTypes.Barrel:
			builder.myGun.barrelLevel = curLevel;
			break;
		case PartTypes.Scope:
			builder.myGun.scopeLevel = curLevel;
			break;
		case PartTypes.Stock:
			builder.myGun.stockLevel = curLevel;
			break;
		case PartTypes.Magazine:
			builder.myGun.magazineLevel = curLevel;
			break;
		}
		//builder.BuildGun ();
		partLevel.text = curLevel.ToString ();
		partType.text = curType.ToString ();
		builder.StartCoroutine("BuildGun");
	}
}
