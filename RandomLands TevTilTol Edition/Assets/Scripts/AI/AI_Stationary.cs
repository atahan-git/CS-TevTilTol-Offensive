using UnityEngine;
using System.Collections;

public class AI_Stationary : MonoBehaviour {

    public GameObject teleportEffect;
    int wallRange = 65;
    Transform wallCenter;

	// Use this for initialization
	void Start () {

        wallCenter = GameObject.FindGameObjectWithTag("WallCenter").transform;
	
	}
	
	// Update is called once per frame
	void Update () {
        if (!wallCenter)
            return;
        if(Vector3.Distance(transform.position, wallCenter.position) > wallRange)
        {
            TeleportInside();
        }
	}

    void TeleportInside()
    {

        Instantiate(teleportEffect, transform.position, transform.rotation);

        transform.rotation = Quaternion.identity;
        transform.position = GetSpawnPoint().position;

        Instantiate(teleportEffect, transform.position, transform.rotation);

    }

    Transform GetSpawnPoint()
    {

    pickAnotherPoint:

        float radius = Random.Range(50f, 80f);
        float angleInDegrees = Random.Range(0f, 360f);
        Vector2 origin = new Vector2(wallCenter.position.x, wallCenter.position.z);


        // Convert from degrees to radians via multiplication by PI/180        
        float x = (float)(radius * Mathf.Cos(angleInDegrees * Mathf.PI / 180F)) + origin.x;
        float y = (float)(radius * Mathf.Sin(angleInDegrees * Mathf.PI / 180F)) + origin.y;

        Transform toBeReturned = transform;

        toBeReturned.position = new Vector3(x, 50, y);
        RaycastHit hit;

        if (Physics.Raycast(toBeReturned.position, Vector3.down, out hit, 100))
        {
            toBeReturned.position = new Vector3(x, hit.point.y, y);
        }
        else
        {
            goto pickAnotherPoint;
        }

        return toBeReturned;

    }
}
