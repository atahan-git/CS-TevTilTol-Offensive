using UnityEngine;
using System.Collections;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(PathMaster))]
public class PathMasterEditor : Editor {

	public override void OnInspectorGUI () {
		DrawDefaultInspector ();
		PathMaster myTarget = (PathMaster)target;


		if (GUILayout.Button ("Add Point to Origin")) {
			GameObject myPoint = (GameObject)Instantiate (myTarget.pointPrefab, Vector3.zero, Quaternion.identity);
			SetPointValues (myPoint, myTarget);
		}
		if (GUILayout.Button ("Add Point to Object Pos")) {
			GameObject myPoint = (GameObject)Instantiate (myTarget.pointPrefab, myTarget.myPathMaster.transform.position, myTarget.myPathMaster.transform.rotation);
			SetPointValues (myPoint, myTarget);
		}
		if (GUILayout.Button ("Add Point to Last Point")) {
			if (myTarget.points.Length > 0) {
				GameObject myPoint = (GameObject)Instantiate (myTarget.pointPrefab, myTarget.points [myTarget.points.Length - 1].transform.position, myTarget.points [myTarget.points.Length - 1].transform.rotation);
				SetPointValues (myPoint, myTarget);
			}
		}
		if (GUILayout.Button ("Add Point to Sceneview Pos")) {
			Camera temp = SceneView.lastActiveSceneView.camera;
			GameObject myPoint = (GameObject)Instantiate (myTarget.pointPrefab, temp.transform.position, temp.transform.rotation);
			SetPointValues (myPoint, myTarget);
		}


		EditorGUILayout.Separator ();

		if(GUILayout.Button("Set Colors")){
			int n = 0;
			foreach (PathPoint myPoint in myTarget.points) {
				myPoint.gameObject.name = "Point " + n;

				if (n == 0) {
					myPoint.color = myTarget.startColor;
					myPoint.size = myTarget.startSize;

				} else if (n == myTarget.points.Length - 1) {
					myPoint.color = myTarget.endColor;
					myPoint.size = myTarget.endSize;

				} else {
					myPoint.color = myTarget.midColor;
					myPoint.size = myTarget.midSize;
				}

				n++;
			}
		}

		if (GUILayout.Button ("Check-Set NavMesh options")) {
			bool isNavMesh = true;

			if (myTarget.myPathMaster != null) {
				if (myTarget.myPathMaster.nav != null) {
					isNavMesh = true;
				} else {
					isNavMesh = false;
				}
			}


			foreach (PathPoint myPoint in myTarget.points) {
				myPoint.isNavMesh = isNavMesh;
			}
		}

		if (myTarget.gizmos) {
			if(GUILayout.Button("Disable All Gizmos")){
				myTarget.gizmos = false;
				foreach (PathPoint myPoint in myTarget.points) {
					myPoint.gizmos = myTarget.gizmos;
				}

				SceneView.RepaintAll ();
			}
		} else {
			if(GUILayout.Button("Enable All Gizmos")){
				myTarget.gizmos = true;
				foreach (PathPoint myPoint in myTarget.points) {
					myPoint.gizmos = myTarget.gizmos;
				}

				SceneView.RepaintAll ();
			}
		}
	}

	void SetPointValues (GameObject myPoint, PathMaster myTarget){
		myPoint.transform.parent = myTarget.transform;
		myPoint.GetComponent<PathPoint> ().gizmos = true;

		bool isNavMesh = true;

		if (myTarget.myPathMaster != null) {
			if (myTarget.myPathMaster.nav != null) {
				isNavMesh = true;
			} else {
				isNavMesh = false;
			}
		}

		myPoint.GetComponent<PathPoint> ().isNavMesh = isNavMesh;
	}
}
