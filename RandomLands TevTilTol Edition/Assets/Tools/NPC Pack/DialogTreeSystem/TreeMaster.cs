using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TreeMaster : MonoBehaviour {

	public DialogTreeMaster myDialogMaster;

	public AudioSource mySource;
	public Text[] myDisplayArea;

	public Dialog[] dialogs = new Dialog[0];

	public GameObject dialogPrefab;

	[System.Serializable]
	public class MyEventType : UnityEngine.Events.UnityEvent {}
	public MyEventType callWhenBegin;
	public MyEventType callWhenDone;

	// Use this for initialization
	void Start () {
		if (dialogs.Length > 0)
			dialogs [dialogs.Length - 1].callWhenDone.AddListener (EndDialog);
	}
	
	// Update is called once per frame
	public void Update () {
		#if UNITY_EDITOR
		if (Application.isEditor) {
			dialogs = GetComponentsInChildren<Dialog> ();


			int n = 0;
			foreach (Dialog myPoint in dialogs) {
				#if UNITY_EDITOR
				UnityEditor.PrefabUtility.DisconnectPrefabInstance (myPoint);
				#endif
				if (!(myPoint.tag == "" || myPoint.tag == " ")) {
					myPoint.gameObject.name = "Dialog " + n + " - " + myPoint.tag;
				} else {
					myPoint.gameObject.name = "Dialog " + n;
				}
				n++;
				myPoint.myMaster = this;
				if (n < dialogs.Length) {
					myPoint.NextInChain = dialogs [n];
				} else {
					myPoint.NextInChain = null;
					//print ("Added listener");
				}
			}
		}else{
			this.enabled = false;
		}
		#endif
	}

	public void StartDialog () {
		print (gameObject.name + " started");
		dialogs [0].StartDialog ();

		callWhenBegin.Invoke ();

	}

	public void EndDialog () {
		print (gameObject.name + " ended");
		callWhenDone.Invoke ();
	}
}
