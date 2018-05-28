using UnityEngine;
using System.Collections;

public class MovePathMaster : MonoBehaviour {

	public UnityEngine.AI.NavMeshAgent nav;

	bool _gizmos = true;
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
	public PathMaster[] paths = new PathMaster[0];

	public GameObject pathPrefab;

	// Use this for initialization
	void Start () {
		//StartPath (0);
	}
	
	// Update is called once per frame
	void Update () {
		//if(Input.GetKeyDown
	}

	void SetGizmos () {
		foreach (PathMaster myPath in paths) {
			myPath.gizmos = gizmos;
		}
	}
		
	public void StartPath (int pathNumber) {

		paths [pathNumber].StartPath ();
	}
}
