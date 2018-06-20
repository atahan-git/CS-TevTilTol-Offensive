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
	public SyncListInt ping = new SyncListInt();
	public SyncListInt dps = new SyncListInt();

	// Use this for initialization
	void Awake () {
		s = this;
	}
	
	public int AddPlayer(string nick){
		for (int i = 0; i < nicks.Count; i++) {
			if (nicks [i] == nick) {
				return i;
			}
		}

		nicks.Add (nick);
		k.Add (0);
		d.Add (0);
		a.Add (0);
		ping.Add (0);
		dps.Add (0);

		ScoreScreen.s.AddPlayer (nicks.Count-1);
		return nicks.Count-1;
	}


}
