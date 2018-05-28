using UnityEngine;
using System.Collections;

public class XpGiver : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	static public void giveXp (int xpToGive){

		//int xpToGive = (int)(((float)level / 3f + Mathf.Pow((18f / 17f), (float)level) + 10f) * 5f * xpMultiplier);
		 
		XpController.xp.curXp += xpToGive;

	}
}
