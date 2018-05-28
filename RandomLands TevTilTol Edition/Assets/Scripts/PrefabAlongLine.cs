using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PrefabAlongLine : MonoBehaviour {

	#if UNITY_EDITOR

	public GameObject myPrefab;
	public Transform parentObj;

	public float prefabSize = 1f;

	public GameObject ObjectPointA;
	public GameObject ObjectPointB;

	public float gizmoSize = 0.5f;

	Vector3 pointA;
	Vector3 pointB;
	Vector3 checkA;
	Vector3 checkB;


	GameObject[] myObjects = new GameObject[0];
	int size = 0;

	public Quaternion prefabRotation = Quaternion.identity;

	// Use this for initialization
	void Start () {

		if (myPrefab != null && parentObj.childCount <= 1) {
			foreach (Transform chld in parentObj) {
				DestroyImmediate (chld.gameObject);
			}
			foreach (Transform chld in parentObj) {
				DestroyImmediate (chld.gameObject);
			}

			OnValidate ();
		}

		if (Application.isPlaying)
			this.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (myPrefab == null)
			return;

		pointA = ObjectPointA.transform.position;
		pointB = ObjectPointB.transform.position;
		if (pointA != checkA || pointB != checkB) {
			OnValidate ();
		}

		checkA = pointA;
		checkB = pointB;
	}


	void OnValidate () {

		CheckSize ();
		//ResizeObjects ();
	}

	float oldSize;
	float distance;
	void CheckSize () {
		distance = Vector3.Distance (pointA, pointB);

		size = (int)(distance/prefabSize) + 1;
		if (distance != oldSize) {
			DrawPrefabs ();
		}

		oldSize = distance;
	}

	void DrawPrefabs () {
		DestroyOldObjects ();
		myObjects = new GameObject[size];
		for (int i = 0; i < size; i++) {
			myObjects [i] = (GameObject)Instantiate (myPrefab, Vector3.zero, Quaternion.identity);
			myObjects [i].transform.parent = parentObj;
		}

		ResizeObjects ();
	}


	void ResizeObjects () {
		Quaternion objRot = Quaternion.LookRotation (pointA - pointB);
		for (int i = 0; i < size; i++) {
			myObjects [i].transform.position = Vector3.Lerp (pointA, pointB, (1f / ((float)size)) * ((float)i + 1));
			//print ((1f / ((float)size + 1f)) * ((float)i + 1));
			myObjects [i].transform.rotation = objRot;
			//myObjects [i].transform.localRotation *= prefabRotation;
			float s = distance / (prefabSize * (float)size);
			myObjects [i].transform.localScale = new Vector3 (1f,1f,s);
		}
	}


	void DestroyOldObjects () {
		foreach (Transform chld in parentObj) {
			DestroyImmediate (chld.gameObject);
		}
		if (myObjects.GetLength(0) > 0) {
			foreach (GameObject obj in myObjects) {
				if (obj != null)
					DestroyImmediate (obj.gameObject);
			}
		}
	}


	void OnDrawGizmos () {
		if (pointA == null)
			return;

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere (pointA, gizmoSize);

		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere (pointB, gizmoSize);

		Gizmos.color = Color.white;
		Gizmos.DrawLine (pointA, pointB);
	}

	#endif
}
