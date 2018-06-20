using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreKeeper : MonoBehaviour {

	public int myId;

	public Text nick;
	public Text k;
	public Text d;
	public Text a;
	public Text ping;
	public Text dps;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		nick.text = ScoreBoard.s.nicks [myId].ToString();
		k.text = ScoreBoard.s.k [myId].ToString();
		d.text = ScoreBoard.s.d [myId].ToString();
		a.text = ScoreBoard.s.a [myId].ToString();
		ping.text = ScoreBoard.s.ping [myId].ToString();
		dps.text = ScoreBoard.s.dps [myId].ToString() + " DPS";
	}
}
