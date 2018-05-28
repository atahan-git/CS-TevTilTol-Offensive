using UnityEngine;
using System.Collections;

public class ZomgazSpawner : MonoBehaviour {

	GameObject[] spawnPoints;

	public int[] zomgazChance = new int[3];

	public GameObject[] zomgazPrefab;

	Vector2 nextSpawnTime = new Vector2(5f,8f);

	// Use this for initialization
	void Start () {
		spawnPoints = GameObject.FindGameObjectsWithTag ("Respawn");
		InvokeRepeating ("SpawnZomgaz", 1f, Random.Range(nextSpawnTime.x,nextSpawnTime.y));
		zomgazChance [0] = 20;
		zomgazChance [1] = 0;
		zomgazChance [2] = 0;
	}
	
	// Update is called once per frame
	void Update () {


	
	}

	void SpawnZomgaz () {

		nextSpawnTime = new Vector2 (nextSpawnTime.x * 0.9f, nextSpawnTime.y * 0.95f);

		//choose which level to spawn
		int allChance = zomgazChance [0] + zomgazChance [1] + zomgazChance [2];
		int random = Random.Range (0, allChance);

		int curLevel = 0;
		if (random < zomgazChance [0]) {
			curLevel = 0;
			zomgazChance [1]++;
		} else if (random < zomgazChance [0] + zomgazChance [1]) {
			curLevel = 1;
			zomgazChance [2]++;
		} else if (random < zomgazChance [0] + zomgazChance [1] + zomgazChance [2]) {
			curLevel = 2;
		}

		Transform mySpawn = spawnPoints [Random.Range (0, spawnPoints.Length)].transform;

		Instantiate (zomgazPrefab[curLevel], mySpawn.position, mySpawn.rotation);

	}
}
