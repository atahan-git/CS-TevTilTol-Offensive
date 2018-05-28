using UnityEngine;
using System.Collections;

public class BossMovement : MonoBehaviour {

	public int level = 1;

	public int xp = 50;
	public int xpRange = 25;

	public float friendlyFireMultiplier = 1f;
	public GameObject itemDrop;
	public float itemDropChance = 8;
	public GameObject fire;
	public Transform firePos;
	bool isBurning = false;

	public GameObject exp;
	public Rigidbody zomKafa;
	Hp Hpin;
	GameObject player;       
	UnityEngine.AI.NavMeshAgent nav; 
	float distance;
	public float visionDist = 40;
	//bool idle = true;
	public float radious = 30.0f;
	public float power = 1000.0f;
	public float damage = 400f;
	public float maxRange = 8f;
	public float burnRange = 4f;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
