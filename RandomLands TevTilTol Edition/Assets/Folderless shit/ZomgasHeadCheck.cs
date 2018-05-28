using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class ZomgasHeadCheck : MonoBehaviour, IValue {

	public static bool leActive = false;

	public static UnityEvent glowCallON = new UnityEvent();
	public static UnityEvent glowCallOFF = new UnityEvent();

	public float curValue {
		get{
			return leValue;
		}
		set{
			if (isActive) {
				HintScript.isPickedHead = true;
				leValue = value;
			}
		}
	}

	public float leValue;

	public bool isActive {
		get {
			return _isActive;
		}
		set {
			_isActive = value;
			leActive = _isActive;

			Check ();
		}
	}
	bool _isActive = false;

	// Use this for initialization
	void Start () {

	}

	bool oldCheck = false;

	// Update is called once per frame
	void Update () {

		/*if (oldCheck != isActive) {

			if (isActive) {
				glowCallON.Invoke ();

			} else {
				glowCallOFF.Invoke ();
			}
		}*/
		//curValue = leValue;

	}

	void Check (){
		if (_isActive)
			glowCallON.Invoke ();
		else
			glowCallOFF.Invoke ();
	}
}
