using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour {

	public Transform player;
	GameObject[] spawnPoints;

	public int[] diffEnemySpawnChance = new int[3];
	//0 = zomgaz
	//1 = sawtank
	//2 = rollergun

	public int[] zomgazChance = new int[3];
	public int[] sawtankChance = new int[3];
	public int[] rollergunChance = new int[3];

	//public GameObject[][] allObjectPools;
	public GameObject[] zomgazPrefab;
	public GameObject[] sawtankPrefab;
	public GameObject[] rollergunPrefab;

	Vector2 nextSpawnTime = new Vector2(5f,8f);

	public int curWave = 1;

	public int enemyCount = 7;
	public float timeBWaveEnenmies = 0.2f;
	public float waveTime = 20f;
	public float restTime = 15f;

	public GameObject spawnEffect;

	public Image waveFlash;

	public bool isRepeatedSpawnFinished = false;
	public bool isContinuousSpawnFinished = false;
	// Use this for initialization
	void Start () {
		spawnPoints = GameObject.FindGameObjectsWithTag ("Respawn");

		Invoke ("Begin", 3f);
	}

	// Update is called once per frame
	void Update () {
		

	}

	void Begin(){
		StartCoroutine (FlashWave ());
		StartCoroutine (StartWave ());
	}

	IEnumerator StartWave (){

		print ("Wave has begun");

		StartCoroutine(RepeatedSpawnEnemies (enemyCount/2));

		while(!isRepeatedSpawnFinished){
			//yield return new WaitForSeconds (waveTime/3f);
			yield return 0;
		}
		isRepeatedSpawnFinished = false;

		StartCoroutine(ContinuousSpawnEnemies (enemyCount, waveTime));

		while (!isContinuousSpawnFinished) {
			//yield return new WaitForSeconds (waveTime * 6f / 5f);
			yield return 0;
		}
		isContinuousSpawnFinished = false;

		StartCoroutine(RepeatedSpawnEnemies ((int)((float)enemyCount*2f/3f)));

		CalcRestTime ();
		CalcWaveTime ();
		CalcEnemyCount ();
		curWave++;

		waveFlash.color = Color.red;
		yield return new WaitForSeconds (restTime);
		StartCoroutine (StartWave ());
		StartCoroutine (FlashWave ());
	}



	IEnumerator RepeatedSpawnEnemies (int spawnEnemyCount){

		print ("Repeated spawn started");

		//int waveStarted = curWave;

		for (int i = 1; i <= spawnEnemyCount; i++) {

			SpawnEnemy ();
			yield return new WaitForSeconds (timeBWaveEnenmies);

		}
		isRepeatedSpawnFinished = true;
		//print ("Repeated spawn finished" + waveStarted + " - " + curWave);
	}

	IEnumerator ContinuousSpawnEnemies (int spawnEnemyCount, float timeSpan){

		print ("Continious spawn started");

		//int waveStarted = curWave;

		for (int i = 1; i <= spawnEnemyCount; i++) {

			SpawnEnemy ();
			yield return new WaitForSeconds (timeSpan/(float)spawnEnemyCount);

		}
		isContinuousSpawnFinished = true;
		//print ("Continious spawn finished" + waveStarted + " - " + curWave);

	}

	void CalcRestTime (){
		restTime *= 1.1f;
	}
	void CalcWaveTime (){
		waveTime *= 1.1f;
	}
	void CalcEnemyCount (){
		enemyCount = (int)(enemyCount * 1.2f);
	}

	void SpawnEnemy () {

		GameObject[] enemyPool;
		int[] chancePool;
		int enemyLevel = 0;
		int rollDice = 0;

		//chose which enemy to spawn
		foreach (int i in diffEnemySpawnChance) {
			rollDice += i;
		}
		rollDice = Random.Range (0, rollDice);
		//print (rollDice);

		if(rollDice < diffEnemySpawnChance[0]){
			enemyPool = zomgazPrefab;
			chancePool = zomgazChance;
			diffEnemySpawnChance [1] += 2;
			diffEnemySpawnChance [2] += 2;
		}else if(rollDice < diffEnemySpawnChance[0] + diffEnemySpawnChance[1]){
			enemyPool = sawtankPrefab;
			chancePool = sawtankChance;
			diffEnemySpawnChance [0] += 4;
			diffEnemySpawnChance [1] += 2;
		}
		else{
			enemyPool = rollergunPrefab;
			chancePool = rollergunChance;
			diffEnemySpawnChance [0] += 4;
			diffEnemySpawnChance [2] += 2;
		}

		rollDice = 0;
		//choose which level to spawn
		foreach (int i in chancePool) {
			rollDice += i;
		}
		rollDice = Random.Range (0, rollDice);

		//spawn it
		if (rollDice < chancePool [0]) {
			enemyLevel = 0;
			chancePool [1]++;
		} else if (rollDice < chancePool [0] + chancePool [1]) {
			enemyLevel = 1;
			chancePool [2]++;
			chancePool [0]--;
		} else if (rollDice < chancePool [0] + chancePool [1] + chancePool [2]) {
			enemyLevel = 2;
			chancePool [1]--;
		} else {
			Debug.LogError ("wtf happened " + rollDice + "  " + (chancePool [0] + chancePool [1] + chancePool [2]));
		}

		//Transform mySpawn = spawnPoints [Random.Range (0, spawnPoints.Length)].transform;
		Transform mySpawn = GetSpawnPoint();

		Instantiate (enemyPool[enemyLevel], mySpawn.position, mySpawn.rotation);
		Instantiate (spawnEffect, mySpawn.position, mySpawn.rotation);

		for (int ii = 0; ii < chancePool.Length; ii++) {
			chancePool[ii] = (int)Mathf.Clamp (chancePool[ii], 0, Mathf.Infinity);
		}

	}

	IEnumerator FlashWave (){
		for (int i = 0; i <= 3; i++) {

			waveFlash.color = Color.green;
			yield return new WaitForSeconds (0.2f);

			waveFlash.color = Color.red;
			yield return new WaitForSeconds (0.2f);

		}
		waveFlash.color = Color.green;
	}

	Transform GetSpawnPoint(){

		float radius = Random.Range (50f, 80f);
		float angleInDegrees = Random.Range (0f, 360f);
		Vector2 origin = new Vector2 (player.position.x, player.position.z);


			// Convert from degrees to radians via multiplication by PI/180        
		float x = (float)(radius * Mathf.Cos(angleInDegrees * Mathf.PI / 180F)) + origin.x;
		float y = (float)(radius * Mathf.Sin(angleInDegrees * Mathf.PI / 180F)) + origin.y;

		Transform toBeReturned = transform;

		toBeReturned.position = new Vector3 (x, 20, y);
		RaycastHit hit;

		if (Physics.Raycast (toBeReturned.position, Vector3.down, out hit, 50)) {
			toBeReturned.position = new Vector3 (x, hit.point.y, y);
		}

		return toBeReturned;

	}
}
