using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ObjectiveGUI : MonoBehaviour {

	[System.NonSerialized]
	public Objective myObjective;

	//reference
	/*public enum conditionType{
		ifTrue,
		ifFalse,
		ifAll,
		ifAny,
		ifLessThan,
		ifMoreThan,
		ifEqual
	}*/

	public GameObject ifTrue;
	public GameObject ifFalse;
	public GameObject ifAll;
	public GameObject ifAny;
	public GameObject ifLessThan;
	public GameObject ifMoreThan;
	public GameObject ifEqual;


	Slider slider;
	Toggle toggle;
	Image image;
	Text text;

	GameObject toSpawn;

	// Use this for initialization
	void Start () {

		switch (myObjective.type) {
		case Objective.conditionType.ifTrue:
			toSpawn = ifTrue;
			break;
		case Objective.conditionType.ifFalse:
			toSpawn = ifFalse;
			break;
		case Objective.conditionType.ifAll:
			toSpawn = ifAll;
			break;
		case Objective.conditionType.ifAny:
			toSpawn = ifAny;
			break;
		case Objective.conditionType.ifLessThan:
			toSpawn = ifLessThan;
			break;
		case Objective.conditionType.ifMoreThan:
			toSpawn = ifMoreThan;
			break;
		case Objective.conditionType.ifEqual:
			toSpawn = ifEqual;
			break;
		}

		if (toSpawn) {
			GameObject spawned = (GameObject)Instantiate (toSpawn, transform.position, transform.rotation);
			spawned.GetComponent<RectTransform>().SetParent(this.transform);
			spawned.transform.localPosition = Vector3.zero;
			spawned.transform.localRotation = Quaternion.identity;
			spawned.transform.localScale = new Vector3 (1, 1, 1);
			slider = spawned.GetComponent<Slider> ();
			toggle = spawned.GetComponent<Toggle> ();
		}

		image = GetComponent<Image> ();
		text = GetComponentInChildren<Text> ();
		text.text = myObjective.name;
	
	}
	
	// Update is called once per frame
	void Update () {
		
		if (toggle) {
			if (myObjective.checkerI.curValue == 1)
				toggle.isOn = true;
			else
				toggle.isOn = false;
		} else if (slider) {
			slider.maxValue = myObjective.maxValue;
			slider.value = myObjective.checkerI.curValue;
		}

		if (myObjective.isDone)
			Done ();
		else
			NotDone ();
	}

	public void Done(){
		image.color = Color.green;
	}
	public void NotDone(){
		image.color = Color.white;
	}
}


/*
		switch (myObjective.type) {
		case Objective.conditionType.ifTrue:
			
			break;
		case Objective.conditionType.ifFalse:
			
			break;
		case Objective.conditionType.ifAll:
			
			break;
		case Objective.conditionType.ifAny:
			
			break;
		case Objective.conditionType.ifLessThan:
			
			break;
		case Objective.conditionType.ifMoreThan:
			
			break;
		case Objective.conditionType.ifEqual:
			
			break;
		}
*/