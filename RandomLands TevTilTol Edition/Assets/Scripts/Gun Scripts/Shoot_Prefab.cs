using UnityEngine;
using System.Collections;

public class Shoot_Prefab : MonoBehaviour {

	[HideInInspector]
	public GunSharedValues val;

	public GameObject bulletPrefab;
	public GameObject muzzleFlash;
	//AudioSource audio;

	[HideInInspector]
	public float z = 10f;
	// Use this for initialization
	void Start () {

		if (val == null)
			val = GetComponent<GunSharedValues> ();
		if (val == null)
			val = GetComponentInParent<GunSharedValues> ();
		if (val == null)
			val = GetComponentInChildren<GunSharedValues> ();


	}

	// Update is called once per frame
	void Shoot(){

		Transform barrelPoint = val.barrelPoint;

		//  The Ray-hits will be in a circular area
		float randomRadius = Random.Range (0, val.accuracy);

		float randomAngle = Random.Range (0, 2 * Mathf.PI);

		//Calculating the raycast direction
		Vector3 direction = new Vector3 (
			randomRadius * Mathf.Cos (randomAngle),
			randomRadius * Mathf.Sin (randomAngle),
			z
		);

		//Make the direction match the transform
		//It is like converting the Vector3.forward to transform.forward
		direction = val.myBulletSource.TransformDirection (direction.normalized);

		Quaternion qDirection = Quaternion.LookRotation (direction);


		Instantiate (bulletPrefab, barrelPoint.position, qDirection);
		Instantiate (muzzleFlash, barrelPoint.position, qDirection);


	}
}
