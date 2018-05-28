using UnityEngine;
using System.Collections;

public class AI_FollowScript : MonoBehaviour {

	GameObject player;       
	UnityEngine.AI.NavMeshAgent nav; 
	float distance;
	public float visionDist = 400;
	public float stoppingDistance = 0;
	[Range (0.5f, 20)]
	public float maxSpeed = 10;
	[Range (1f, 20)]
	public float acceleration = 5;

    float stoppingDistanceMEM = 0;
    GunSharedValues gunS;

    // Use this for initialization
    void Start () {

        stoppingDistanceMEM = stoppingDistance;
        gunS = GetComponentInChildren<GunSharedValues>();
        player = GameObject.FindGameObjectWithTag ("Player");
		nav = GetComponent<UnityEngine.AI.NavMeshAgent> ();
		
		float speedMultiplier = GameSpeedChanger.monsterSpeedMult;
		nav.speed = maxSpeed * speedMultiplier;
		nav.acceleration = acceleration * speedMultiplier;
		
		nav.stoppingDistance = stoppingDistance;

        if(gunS)
            InvokeRepeating("CheckIfCanSeePlayer", 0.1f, 0.2f);
	
	}
	
	// Update is called once per frame
	void Update () {

		if(player == null)
			player = GameObject.FindGameObjectWithTag("Player");
		if (player == null)
			return; //player does not exist

		distance = Vector3.Distance(transform.position, player.transform.position);

		if(distance > visionDist){

			nav.enabled = false;
		}else{
			nav.enabled = true;
			if(nav.isOnNavMesh)
				nav.SetDestination (player.transform.position);
		}
	
	}

    void CheckIfCanSeePlayer()
    {
        //+ (gunS.myBulletSource.forward * 2)
        if (player == null)
            return; //player does not exist

        Transform checkPoint = gunS.barrelPoint;
        Ray myRay = new Ray(checkPoint.position , player.transform.position - checkPoint.position);
        RaycastHit hit;

        if (Physics.Raycast(myRay, out hit)){
            Debug.DrawLine(checkPoint.position, hit.point);
            if (hit.transform.root == player.transform || hit.transform.root.gameObject.name == "Ufo & Walls")
            {
                //can see the player
                stoppingDistance = stoppingDistanceMEM;
                nav.stoppingDistance = stoppingDistance;
                //print("Can see him");
            }
            else
            {
                //cant see the player
                stoppingDistance = 0f;
                nav.stoppingDistance = stoppingDistance;
                //print("Can not see him");
            }
        }
    }
}
