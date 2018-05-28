using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Dialog : MonoBehaviour {

	public TreeMaster myMaster;

	[SerializeField]
	string _tag;

	public string tag{
		get{
			return _tag;
		}
		set{
			_tag = value;
			//print (_tag);
			myMaster.Update ();
			//print ("this Called");
		}
	}

	public AudioClip soundFile;	
	public AudioSource source;	//If left null will check master until finds one and uses it instead
	public Text[] displayArea; //If left null will check master until finds one and uses it instead

	[TextArea]
	public string text;

	[Header("Go for 0.5s offset and 4w/s for normal talking")]
	public bool autoSetDuration = true;
	public float duration = 2f;
	public float preOffset = 0.15f;
	public float latOffset = 0.15f;

	[System.Serializable]
	public class MyEventType : UnityEngine.Events.UnityEvent {}
	public MyEventType callWhenBegin;
	public MyEventType callWhenDone;

	public Dialog NextInChain;
	public bool breakAutoChain = false;

	bool isStarted = false;

	bool audioEnabled = true;
	bool textEnabled = true;
	string optionalPlayerprefSubtitleDisableOption = "Subtitles"; //set this value 1 or 0 based on setting if you want to

	// Use this for initialization
	void Start () {
		//Check audio system values
		if (soundFile != null) {
			if (source == null) {
				source = myMaster.mySource;
				if (source == null) {
					source = myMaster.myDialogMaster.mySource;
				}
				if (source == null) {
					audioEnabled = false;
				}
			}
		} else {
			audioEnabled = false;
		}
		//print (audioEnabled);

		//Check text system values
		if (!(text == "" || text == " ")) {
			if (displayArea.Length < 1) {
				displayArea = myMaster.myDisplayArea;
				if (displayArea.Length < 1) {
					displayArea = myMaster.myDialogMaster.myDisplayArea;
				}
				if (displayArea.Length < 1) {
					textEnabled = false;
				}
			}
		} else {
			textEnabled = false;
		}

		//autoset values
		if (autoSetDuration) {
			if (soundFile != null) {
				
				duration = soundFile.length;


			} else if (!(text == "" || text == " ")) {

				string temp = text;

				int numberOfWords = temp.Split (new[]{ ' ', '\t' }).Length;

				duration = (float)numberOfWords / 3f;

			}
		}

		//print (textEnabled);
	}
	
	// Update is called once per frame
	public void StartDialog () {
		if (!isStarted) {
			if (textEnabled)
				Invoke ("SetText", preOffset);
			if (audioEnabled)
				Invoke ("SetAudio", preOffset);
		}
		isStarted = true;

		callWhenBegin.Invoke ();
	}

	void SetText () {
		ChangeAllText (displayArea, text);
		Invoke ("UnSetText", duration + latOffset);
	}

	void UnSetText () {
		ChangeAllText (displayArea, "");
		EndDialog ();
	}

	void SetAudio () {
		source.clip = soundFile;
		source.Play ();
	}

	public void EndDialog () {
		isStarted = false;
		if (NextInChain != null && !breakAutoChain) {
			NextInChain.StartDialog ();
		}
		if (callWhenDone != null) {
			callWhenDone.Invoke ();
		}
	}


	void ChangeAllText (Text[] myText, string myVal){
		foreach (Text theText in myText) {
			if (theText != null) {
				//print (theText.gameObject.name);
				theText.text = myVal;

			}
		}
	}
}
