using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DialogTreeMaster : MonoBehaviour {

	public AudioSource mySource;
	public bool autoSetAudioSource = true;
	public Text[] myDisplayArea;

	public TreeMaster[] trees = new TreeMaster[0];

	public GameObject treePrefab;

	// Use this for initialization
	void Awake () {
		if (autoSetAudioSource)
			mySource = GetComponent<AudioSource> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void StartDialog (int dialogNumber) {

		trees [dialogNumber].StartDialog ();
	}
}
