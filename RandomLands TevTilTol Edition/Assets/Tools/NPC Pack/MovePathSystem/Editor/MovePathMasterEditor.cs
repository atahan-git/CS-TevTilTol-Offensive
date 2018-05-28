using UnityEngine;
using System.Collections;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(MovePathMaster))]
public class MovePathMasterEditor : Editor {

	public override void OnInspectorGUI () {
		DrawDefaultInspector ();
		MovePathMaster myTarget = (MovePathMaster)target;

		//instantiate object, change its name, add to the array

		if (GUILayout.Button ("Add Path to Player Pos")) {
			GameObject myObject = (GameObject)Instantiate (myTarget.pathPrefab, myTarget.transform.position, myTarget.transform.rotation);
			myObject.name = myTarget.gameObject.name + " - Path " + (myTarget.paths.Length).ToString();
			myObject.GetComponent<PathMaster> ().myPathMaster = myTarget;
			AddToArray (myTarget.paths, myObject.GetComponent<PathMaster> (), myTarget);
		}

		if (GUILayout.Button ("Add Path to Origin")) {
			GameObject myObject = (GameObject)Instantiate (myTarget.pathPrefab, Vector3.zero, Quaternion.identity);
			myObject.name = myTarget.gameObject.name + " - Path " + (myTarget.paths.Length).ToString();
			myObject.GetComponent<PathMaster> ().myPathMaster = myTarget;
			AddToArray (myTarget.paths, myObject.GetComponent<PathMaster> (), myTarget);
		}
		if (GUILayout.Button ("Add Path to Sceneview Pos")) {
			Camera temp = SceneView.lastActiveSceneView.camera;
			GameObject myObject = (GameObject)Instantiate (myTarget.pathPrefab, temp.transform.position, temp.transform.rotation);
			myObject.name = myTarget.gameObject.name + " - Path " + (myTarget.paths.Length).ToString();
			myObject.GetComponent<PathMaster> ().myPathMaster = myTarget;
			AddToArray (myTarget.paths, myObject.GetComponent<PathMaster> (), myTarget);
		}

		EditorGUILayout.Separator ();

		if (myTarget.gizmos) {
			if(GUILayout.Button("Disable All Gizmos")){
				myTarget.gizmos = false;
				foreach (PathMaster myPoint in myTarget.paths) {
					myPoint.gizmos = myTarget.gizmos;
				}

				SceneView.RepaintAll ();
			}
		} else {
			if(GUILayout.Button("Enable All Gizmos")){
				myTarget.gizmos = true;
				foreach (PathMaster myPoint in myTarget.paths) {
					myPoint.gizmos = myTarget.gizmos;
				}

				SceneView.RepaintAll ();
			}
		}

		if (GUILayout.Button ("Get NavMesh")) {
			myTarget.nav = myTarget.GetComponent<UnityEngine.AI.NavMeshAgent> ();
		}

		//GUILayout.Label ("Be careful with this button, There is no Undo!");
		EditorGUILayout.LabelField("Be careful with this button, There is no Undo!", EditorStyles.boldLabel);

		if (GUILayout.Button ("Remove All Paths")) {
			foreach (PathMaster myPath in myTarget.paths) {
				if (myPath != null) {
					DestroyImmediate (myPath.gameObject);
				}
			}

			myTarget.paths = new PathMaster[0];
		}
	}



	void AddToArray (PathMaster[] myArray,PathMaster toAdd, MovePathMaster myTarget) {
		//Debug.Log ("this called");
		PathMaster[] oldArray = new PathMaster[myArray.Length];
		myArray.CopyTo (oldArray, 0);

		myArray = new PathMaster[myArray.Length + 1];

		oldArray.CopyTo (myArray,0);

		myArray [myArray.Length - 1] = toAdd;

		myTarget.paths = myArray;
		 
	}
}
