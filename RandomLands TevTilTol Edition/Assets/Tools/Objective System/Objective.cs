using UnityEngine;
using System.Collections;

[System.Serializable]
public class Objective {

	public bool isEnabled = false;
	public bool isDone = false;
	public bool onlyTriggerOnce = true;
	public bool isTriggeredOnce = false;

	public string name;

	public enum conditionType{
		ifTrue,
		ifFalse,
		ifAll,
		ifAny,
		ifLessThan,
		ifMoreThan,
		ifEqual
	}
	public conditionType type;

	//use 0 and 1 for booleans
	public float requiredValue = 1;
	public float maxValue = 1; //you can leave this empty for booleans
	//access current value from checkerI.value

	//this script will check our target objective and give us values
	//switch this script for different functionality
	public MonoBehaviour checker;
	public IValue checkerI {
		get {
			return (IValue)checker;
		}
	}

	[System.Serializable]
	public class MyEventType : UnityEngine.Events.UnityEvent {}
	public MyEventType callWhenDone;


	public GameObject GUI;

	//this value will be edited by the "checker"
	//[System.NonSerialized]
	//public float curValue;

}


public interface IValue {
	float curValue { get; set; }
	bool isActive { get; set; }
}