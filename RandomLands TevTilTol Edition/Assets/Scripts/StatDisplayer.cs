using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StatDisplayer : MonoBehaviour {

	public enum Stat{maxAccuracy, decreaseAccuracy, increaseAccuracy, leastAccuracy, ammoCapacity, reloadSeconds, damage, fireRate};
	public Stat myStat;

	float myStatVal = 0f;

	public Text statName;
	public Text statValue;

	GunController myGunCont;

	// Use this for initialization
	void Start () {

		//set name
		switch (myStat) {
		case Stat.maxAccuracy:
			statName.text = "Max Acc";
			break;
		case Stat.decreaseAccuracy:
			statName.text = "Dec Acc";
			break;
		case Stat.increaseAccuracy:
			statName.text = "Inc Acc";
			break;
		case Stat.leastAccuracy:
			statName.text = "Lst Acc";
			break;
		case Stat.ammoCapacity:
			statName.text = "Ammo";
			break;
		case Stat.reloadSeconds:
			statName.text = "Rld S";
			break;
		case Stat.damage:
			statName.text = "Dmg/DPS";
			break;
		case Stat.fireRate:
			statName.text = "BPM";
			break;
		}
	
	}
	
	// Update is called once per frame
	void Update () {

		if(myGunCont == null)
			myGunCont = GunController.myGunCont;


		UpdateStats ();
	}

	//public enum Stat{maxAccuracy, decreaseAccuracy, increaseAccuracy, leastAccuracy, ammoCapacity, reloadSeconds, damage, fireRate};
	void UpdateStats(){

		switch (myStat) {
		case Stat.maxAccuracy:
			myStatVal = myGunCont.maxAccuracy;
			break;
		case Stat.decreaseAccuracy:
			myStatVal = myGunCont.decreaseAccuracy;
			break;
		case Stat.increaseAccuracy:
			myStatVal = myGunCont.increaseAccuracy;
			break;
		case Stat.leastAccuracy:
			myStatVal = myGunCont.leastAccuracy;
			break;
		case Stat.ammoCapacity:
			myStatVal = myGunCont.ammoCapacity;
			break;
		case Stat.reloadSeconds:
			myStatVal = myGunCont.reloadSeconds;
			break;
		case Stat.damage:
			float lel = (float)myGunCont.damage * myGunCont.fireRate/60f;
			myStatVal = myGunCont.damage;
			statValue.text = myStatVal + "/" + lel.ToString();
			return;
			break;
		case Stat.fireRate:
			myStatVal = myGunCont.fireRate;
			break;
		}

		statValue.text = myStatVal.ToString ();

	}

}
