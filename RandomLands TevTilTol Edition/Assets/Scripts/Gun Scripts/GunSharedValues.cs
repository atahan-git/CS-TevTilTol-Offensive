using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GunSharedValues : NetworkBehaviour {

	public bool isPlayer = false;

	[HideInInspector]
	public float accuracy = 0.5f;
	[HideInInspector]
	public int damage = 5;

	public Transform[] barrelPoints = new Transform[1];

	public Transform barrelPoint{
		get{
			return barrelPoints[Random.Range(0, barrelPoints.Length)];
		}
		set{
			barrelPoints[0] = value;
		}
	}
	
	[Header("Leave empty if shooting from barrel point")]
	public Transform myBulletSource;

	// Use this for initialization
	void Start () {
		if(barrelPoints.Length == 0)
			barrelPoints = new Transform[1];

		if (myBulletSource == null && isPlayer && isLocalPlayer) {
			myBulletSource = GetComponent<PlayerRelay> ().myPlayer.transform.GetChild (0);
		} else {
			myBulletSource = transform;
		}

		if (myBulletSource == null) {
			myBulletSource = barrelPoint;
		}
	
	}
	
	// Update is called once per frame
	void Update () {
		if (barrelPoint == null) {
			barrelPoint = GetComponent<GunBuilder> ().myBarrelPoint;
		}
	}
}
