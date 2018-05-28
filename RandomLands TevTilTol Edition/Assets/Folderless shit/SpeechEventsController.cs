using UnityEngine;
using System.Collections;

public class SpeechEventsController : MonoBehaviour {

	DialogTreeMaster dialog;

	V2_EnemySpawner spawner;

	public int triggeredWave = 2;
	public int lastDialog = 1;

	// Use this for initialization
	void Start () {
		dialog = GetComponent<DialogTreeMaster> ();
		dialog.StartDialog (0);

		spawner = GameObject.FindObjectOfType<V2_EnemySpawner> ();

		triggeredWave = spawner.curWave + 1;

	}

	bool isAllDone = false;

	public void DoneAllMissions (){
		isAllDone = true;
	}

	bool isEndTriggered = false;

	// Update is called once per frame
	void Update () {
		if (spawner.curWave == triggeredWave) {
			if (triggeredWave < 5) {
				dialog.StartDialog (lastDialog);
			}

			if (spawner.curWave >= 10 && isAllDone && !isEndTriggered) {
				dialog.StartDialog (7);
				isEndTriggered = true;
			}

			if (triggeredWave == 14) {
				dialog.StartDialog (8);
				EndGameRocketController.s.Engage ();
			}
			if (triggeredWave == 16) {
				dialog.StartDialog (9);
			}

			triggeredWave++;
			lastDialog++;
		}
	}
}
