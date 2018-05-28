using UnityEngine;
using System.Collections;

public class PathPoint : MonoBehaviour {
	
	//----------------------------------------------------------------Custom Inspector
	//[HideInInspector]
	[SerializeField]
	public PathMaster myMaster;

	//[HideInInspector]
	[SerializeField]
	string _tag;
	//[HideInInspector]
	public string tag{
		get{
			return _tag;
		}
		set{
			_tag = value;
			//print (_tag);
			myMaster.Update ();
			//print ("this Called");
		}
	}

	//[HideInInspector]
	[SerializeField]
	public bool isNavMesh = true; //lerp or normal movement
	[Header("If isNavMesh is true these two doesn't matter")]
	//[HideInInspector]
	[SerializeField]
	public bool isTowardsMove = true; //moveTowards or lerp
	//[HideInInspector]
	[SerializeField]
	public float speedMove = 2f;
	//[HideInInspector]
	[SerializeField]
	public bool isTowardsRot = false; //moveTowards or lerp
	//[HideInInspector]
	[SerializeField]
	public float speedRot = 10f;

	[Header("Reach and Wait Options")]
	//navmesh stuff
	//[HideInInspector]
	[SerializeField]
	public bool endWhenReachedPos = true;
	//[HideInInspector]
	[SerializeField]
	public float reachThresholdPos = 2f;
	//normal stuff
	//[HideInInspector]
	[SerializeField]
	public bool endWhenReachedRot = false;
	//[HideInInspector]
	[SerializeField]
	public float reachThresholdRot = 5f;
	//----------------------------------------------------------------Custom Inspector

	//[HideInInspector]
	[SerializeField]
	public bool endWhenNoTime = false;
	//[HideInInspector]
	[SerializeField]
	public float time = 2f;

	public float waitBeforeBegin = 0f;


	float curTime;
	bool isMoving;


	[System.Serializable]
	public class MyEventType : UnityEngine.Events.UnityEvent {}
	public MyEventType callWhenBegin;
	public MyEventType callWhenDone;


	public PathPoint NextInChain;
	public bool breakAutoChain = false;

	[Space]
	public bool teleportStart = false;

	[Header("Gizmo Stuff")]
	public bool gizmos = true;
	public float size = 0.5f;
	public Color color = Color.blue;

	public bool endBlendTm = true;
	bool isEnded = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void StartMove () {

		if (teleportStart) {
			myMaster.myPathMaster.transform.position = transform.position;
			myMaster.myPathMaster.transform.rotation = transform.rotation;
		}
		
		if (!isMoving) {
			curTime = 0f;
			Invoke ("WaitMove", waitBeforeBegin);
		}
		isEnded = false;
		isMoving = true;

		callWhenBegin.Invoke ();
	}

	public void EndMove () {
		isMoving = false;

		if (isEnded)
			return;
		isEnded = true;
		if (NextInChain != null && !breakAutoChain) {
			NextInChain.StartMove ();
		}
		if (callWhenDone != null) {
			callWhenDone.Invoke ();
		}
	}

	void WaitMove () {
		StartCoroutine (Move());
	}

	IEnumerator Move () {

		if (NextInChain == null) {
			yield return null;
			EndMove ();
			yield break;
		}

		if (!endWhenNoTime) {
			curTime = -1f;
			time = 1f;
		}

		if (myMaster.myPathMaster.nav != null) {
			if (isNavMesh) {
				myMaster.myPathMaster.nav.enabled = true;
			} else {
				myMaster.myPathMaster.nav.enabled = false;
			}
		}
		float myDistance = 999f;

		while (curTime < time) {
			
			if (endWhenReachedPos) {
				myDistance = Vector3.Distance (myMaster.myPathMaster.transform.position, NextInChain.transform.position);
				if (myDistance < reachThresholdPos) {
					break;
				}
			}


			//Move
			if (isNavMesh) {

				myMaster.myPathMaster.nav.SetDestination (NextInChain.transform.position);

			} else {

				if (endWhenReachedRot) {
					myDistance = Quaternion.Angle (myMaster.myPathMaster.transform.rotation, NextInChain.transform.rotation);
					if (myDistance < reachThresholdRot) {
						break;
					}
				}


				if (isTowardsMove) {
					myMaster.myPathMaster.transform.position = Vector3.MoveTowards (myMaster.myPathMaster.transform.position, NextInChain.transform.position, speedMove * Time.deltaTime);
				} else {
					myMaster.myPathMaster.transform.position = Vector3.Lerp (myMaster.myPathMaster.transform.position, NextInChain.transform.position, speedMove * Time.deltaTime);
				}

				if (isTowardsRot) {
					myMaster.myPathMaster.transform.rotation = Quaternion.RotateTowards (myMaster.myPathMaster.transform.rotation, NextInChain.transform.rotation, speedRot * Time.deltaTime);
				} else {
					myMaster.myPathMaster.transform.rotation = Quaternion.Slerp (myMaster.myPathMaster.transform.rotation, NextInChain.transform.rotation, speedRot * Time.deltaTime);
				}
			}

			if (endWhenNoTime) {
				curTime += Time.deltaTime;
			}

			if (myDistance < reachThresholdPos)
				EndMove ();

			yield return null;
		}
		yield return null;
		EndMove ();
	}

	void OnDrawGizmos () {
		if (gizmos) {
			Gizmos.color = color;
			Gizmos.DrawSphere (transform.position, size);
			Gizmos.color = Color.blue;
			Gizmos.DrawLine (transform.position, transform.position + (transform.forward * size * 2));
			Gizmos.color = Color.red;
			Gizmos.DrawLine (transform.position, transform.position + (transform.right * size * 2));
			Gizmos.color = Color.grey;
			Gizmos.DrawLine (transform.position, transform.position + (transform.up * size * 2));
		}
	}

	void OnDrawGizmosSelected () {
		if (gizmos) {
			Gizmos.color = color;
			if(NextInChain)
				Gizmos.DrawWireSphere (NextInChain.transform.position, reachThresholdPos);
		}
	}
}
