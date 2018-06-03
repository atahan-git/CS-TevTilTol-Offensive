using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ScoreBoard : NetworkBehaviour {

	public static ScoreBoard s;



	public SyncListString nicks = new SyncListString();
	public SyncListInt k = new SyncListInt();
	public SyncListInt d = new SyncListInt();
	public SyncListInt a = new SyncListInt();

	// Use this for initialization
	void Awake () {
		s = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
