using UnityEngine;
using System.Collections;

public class AI_Death : MonoBehaviour {

	Hp Hpin;
	AI_Level lvl;

	public GameObject itemDrop;

	public int xp = 50;
	public int xpRange = 25;

	public bool enemyCounterEnabled = true;

	// Use this for initialization
	void Start () {
		Hpin = GetComponent<Hp> ();
		lvl = GetComponent<AI_Level> ();
	}
	
	// Update is called once per frame
	void Update () {

		if(Hpin.hpi <= 0){
			if (enemyCounterEnabled)
				EnemyCounter.count--;
			BroadcastMessage ("Die");
			Destroy (gameObject);
		}
	
	}


	void Die (){

		//item drop
		//int randomChance = (int)Random.Range(0, itemDropChance);
		//print(randomChance);
		if(/*randomChance == 0*/ LootCounter.ShouldDropLoot(lvl.level)){

			//Debug.LogWarning("item dropped");
			GameObject myItem = (GameObject)Instantiate(itemDrop, transform.position + (Vector3.up * 2), transform.rotation);
            if (myItem.GetComponent<GunDrop>())
                myItem.GetComponent<GunDrop>().MakeGun(lvl.level, lvl.rarity);

		}

        //Debug.LogError("lel");
		//give xp
		int xpToGive = xp + Random.Range (-xpRange, xpRange);
		XpGiver.giveXp (xpToGive);
		ScoreController.myScore.AddScore ((int)((float)xpToGive * Random.Range(1f, Mathf.Sqrt((float)lvl.level))));

	}
}
