using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PathMaster : MonoBehaviour {

	public MovePathMaster myPathMaster;

	bool _gizmos = true;
	[SerializeField]
	public bool gizmos
	{
		get{
			return _gizmos;
		}
		set{
			_gizmos = value;
			SetGizmos ();
		}
	}

	public PathPoint[] points = new PathPoint[0];

	public GameObject pointPrefab;

	public Color pathColor = Color.white;

	public Color startColor = Color.green;
	public Color midColor = Color.red;
	public Color endColor = Color.magenta;

	public float startSize = 0.4f;
	public float midSize = 0.2f;
	public float endSize = 0.4f;

	[System.Serializable]
	public class MyEventType : UnityEngine.Events.UnityEvent {}
	public MyEventType callWhenBegin;
	public MyEventType callWhenDone;

	// Use this for initialization
	void Start () {
		if (points.Length > 0)
			points [points.Length - 1].callWhenDone.AddListener (EndPath);
	}
	
	// Update is called once per frame
	public void Update () {

		if (Application.isEditor) {
			points = GetComponentsInChildren<PathPoint> ();


			int n = 0;
			foreach (PathPoint myPoint in points) {
				#if UNITY_EDITOR
				UnityEditor.PrefabUtility.DisconnectPrefabInstance (myPoint);
				#endif
				if (!(myPoint.tag == "" || myPoint.tag == " ")) {
					myPoint.gameObject.name = "Point " + n + " - " + myPoint.tag;
				} else {
					myPoint.gameObject.name = "Point " + n;
				}
				n++;
				myPoint.myMaster = this;
				if (n < points.Length) {
					myPoint.NextInChain = points [n];
				} else {
					myPoint.NextInChain = null;
					//print ("Added listener");
				}
			}
		}
	}

	public void StartPath () {
		//print (gameObject.name + " started");
		points [0].StartMove ();
		callWhenBegin.Invoke ();

	}

	public void EndPath () {
		//print (gameObject.name + " ended");
		callWhenDone.Invoke ();
	}

	void OnDrawGizmos () {
		if (gizmos) {
			Gizmos.color = pathColor;
			for (int i = 0; i < points.Length - 1; i++) {
				Gizmos.DrawLine (points [i].transform.position, points [i + 1].transform.position);
			}
		}
	}



	void SetGizmos () {
		foreach (PathPoint myPoint in points) {
			myPoint.gizmos = gizmos;
		}
	}
}
