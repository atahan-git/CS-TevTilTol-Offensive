using UnityEngine;
using System.Collections;

public class LootCounter : MonoBehaviour {

	static int[] enemyCountBeforeLoot = new int[5];
	static int maxCount = 15;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	static public bool ShouldDropLoot (int lvl){

		lvl = lvl - 1;
		int ourChance = Random.Range (0, maxCount);

		if (ourChance <= enemyCountBeforeLoot[lvl]) {

			enemyCountBeforeLoot[lvl] = 0;
			return true;
		} else {

			enemyCountBeforeLoot[lvl]++;
			return false;
		}

	}
}
