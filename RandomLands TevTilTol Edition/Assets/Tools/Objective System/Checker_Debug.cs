using UnityEngine;
using System.Collections;

public class Checker_Debug : MonoBehaviour, IValue {

	public float curValue {
		get{
			return leValue;
		}
		set{
			if(isActive)
				curValue = value;
		}
	}

	public float leValue;

	public bool isActive {
		get {
			return _isActive;
		}
		set {
			_isActive = value;
		}
	}
	bool _isActive = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		//curValue = leValue;
	
	}
}
