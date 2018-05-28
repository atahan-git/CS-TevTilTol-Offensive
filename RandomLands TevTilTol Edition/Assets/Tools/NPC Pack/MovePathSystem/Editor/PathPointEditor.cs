using UnityEngine;
using System.Collections;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(PathPoint))]
public class PathPointEditor : Editor {

	/*SerializedProperty tagProp;
	SerializedProperty myMasterProp;
	SerializedProperty isNavMeshProp;
	SerializedProperty isTowardsMoveProp;
	SerializedProperty speedMoveProp;
	SerializedProperty isTowardsRotProp;
	SerializedProperty speedRotProp;
	SerializedProperty endWhenReachedPosProp;
	SerializedProperty reachThresholdPosProp;
	SerializedProperty endWhenReachedRotProp;
	SerializedProperty reachThresholdRotProp;*/

	void OnEnable () {
		/*tagProp = serializedObject.FindProperty ("tag");
		myMasterProp = serializedObject.FindProperty ("myMaster");
		isNavMeshProp = serializedObject.FindProperty ("isNavMesh");
		isTowardsMoveProp = serializedObject.FindProperty ("isTowardsMove");
		speedMoveProp = serializedObject.FindProperty ("speedMove");
		isTowardsRotProp = serializedObject.FindProperty ("isTowardsRot");
		speedRotProp = serializedObject.FindProperty ("speedRot");
		endWhenReachedPosProp = serializedObject.FindProperty ("endWhenReachedPos");
		reachThresholdPosProp = serializedObject.FindProperty ("reachThresholdPos");
		endWhenReachedRotProp = serializedObject.FindProperty ("endWhenReachedRot");
		reachThresholdRotProp = serializedObject.FindProperty ("reachThresholdRot");*/



	}


	public override void OnInspectorGUI () {
		//PathPoint myTarget = (PathPoint)target;
		PathPoint[] myTargets = new PathPoint[targets.Length];
		int n = 0;
		foreach (Object myTar in targets) {
			myTargets [n] = (PathPoint)myTar;
		}
		//-----------------------------------------------------------------------------------Hardcore shit
		//serializedObject.Update();
		foreach (PathPoint myTarget in myTargets) {
			if (myTarget == null)
				return;

			myTarget.myMaster = EditorGUILayout.ObjectField ("My Master", myTarget.myMaster, typeof(PathMaster), true) as PathMaster;
			myTarget.tag = EditorGUILayout.TextField ("Tag", myTarget.tag);
		
			/*if (myTarget.isNavMesh) {
				myTarget.isNavMesh = EditorGUILayout.Toggle ("is Nav Mesh", myTarget.isNavMesh);

				EditorGUILayout.Separator ();

				myTarget.endWhenReachedPos = EditorGUILayout.Toggle ("Position Check", myTarget.endWhenReachedPos);
				if (myTarget.endWhenReachedPos) {
					myTarget.reachThresholdPos = EditorGUILayout.FloatField ("Threshold", myTarget.reachThresholdPos);
				}

				myTarget.endWhenNoTime = EditorGUILayout.Toggle ("Time Check", myTarget.endWhenNoTime);
				if (myTarget.endWhenNoTime) {
					myTarget.reachThresholdPos = EditorGUILayout.FloatField ("Threshold", myTarget.reachThresholdPos);
				}


			} else {
				myTarget.isNavMesh = EditorGUILayout.Toggle ("is Nav Mesh", myTarget.isNavMesh);

				myTarget.isTowardsMove = EditorGUILayout.Toggle ("is Move Towards", myTarget.isTowardsMove);
				myTarget.speedMove = EditorGUILayout.FloatField ("Move Speed", myTarget.speedMove);
				myTarget.isTowardsRot = EditorGUILayout.Toggle ("is Rotate Towards", myTarget.isTowardsRot);
				myTarget.speedRot = EditorGUILayout.FloatField ("Rotation Speed", myTarget.speedRot);

				EditorGUILayout.Separator ();

				myTarget.endWhenReachedPos = EditorGUILayout.Toggle ("Position Check", myTarget.endWhenReachedPos);
				if (myTarget.endWhenReachedPos) {
					myTarget.reachThresholdPos = EditorGUILayout.FloatField ("Threshold", myTarget.reachThresholdPos);
				}
				myTarget.endWhenReachedRot = EditorGUILayout.Toggle ("Rotation Check", myTarget.endWhenReachedRot);
				if (myTarget.endWhenReachedRot) {
					myTarget.reachThresholdRot = EditorGUILayout.FloatField ("Threshold", myTarget.reachThresholdRot);
				}
				myTarget.endWhenNoTime = EditorGUILayout.Toggle ("Time Check", myTarget.endWhenNoTime);
				if (myTarget.endWhenNoTime) {
					myTarget.time = EditorGUILayout.FloatField ("Threshold", myTarget.time);
				}
			}*/

			//EditorGUILayout.Space ();
			EditorGUILayout.Separator ();
			EditorGUILayout.Separator ();
			EditorGUILayout.Separator ();
			//serializedObject.ApplyModifiedProperties();

			//-----------------------------------------------------------------------------------Def stuff
			DrawDefaultInspector ();


			EditorGUILayout.Separator ();

			if (GUILayout.Button ("Calculate for Time")) {
				float myDis = Vector3.Distance (myTarget.transform.position, myTarget.NextInChain.transform.position);
				float myRotDis = Quaternion.Angle (myTarget.transform.rotation, myTarget.NextInChain.transform.rotation);
				if (myTarget.isTowardsMove) {
					myTarget.speedMove = myDis / myTarget.time;
				}

				if (myTarget.isTowardsRot) {
					myTarget.speedRot = myRotDis / myTarget.time;
				}

				myTarget.endWhenReachedPos = false;
				myTarget.endWhenReachedRot = false;
			}

			if (GUILayout.Button ("Stick to Ground")) {
				RaycastHit hit = new RaycastHit ();

				if (Physics.Raycast (myTarget.transform.position, Vector3.down, out hit)) {
					myTarget.transform.position = hit.point;
				}
			}

			if (GUILayout.Button ("Set to Sceneview Pos")) {
				Camera temp = SceneView.lastActiveSceneView.camera;
				myTarget.transform.position = temp.transform.position;
				myTarget.transform.rotation = temp.transform.rotation;
			}

			EditorGUILayout.Separator ();


			if (GUILayout.Button ("Teleport to here")) {
				if (GameObject.Find ("Teleport Undo " + myTarget.myMaster.myPathMaster.gameObject.name.ToString())) {

				} else {
					GameObject teleportUndo = new GameObject();
					//teleportUndo = (GameObject)Instantiate (teleportUndo, myTarget.myMaster.myPathMaster.transform.position, myTarget.myMaster.myPathMaster.transform.rotation);
					teleportUndo.transform.position = myTarget.myMaster.myPathMaster.transform.position;
					teleportUndo.transform.rotation = myTarget.myMaster.myPathMaster.transform.rotation;
					teleportUndo.transform.parent = myTarget.myMaster.myPathMaster.transform.parent;
					teleportUndo.name = "Teleport Undo " + myTarget.myMaster.myPathMaster.gameObject.name.ToString ();
				}

				myTarget.myMaster.myPathMaster.transform.position = myTarget.transform.position;
				myTarget.myMaster.myPathMaster.transform.rotation = myTarget.transform.rotation;
				myTarget.myMaster.myPathMaster.transform.parent = myTarget.transform;
			}

			if (GUILayout.Button ("Undo Teleport")) {

				GameObject teleportUndo = GameObject.Find ("Teleport Undo " + myTarget.myMaster.myPathMaster.gameObject.name.ToString ());

				if (teleportUndo != null) {
					myTarget.myMaster.myPathMaster.transform.position = teleportUndo.transform.position;
					myTarget.myMaster.myPathMaster.transform.rotation = teleportUndo.transform.rotation;
					myTarget.myMaster.myPathMaster.transform.parent = teleportUndo.transform.parent;

					DestroyImmediate (teleportUndo);
				} else {
					Debug.Log ("Teleport Undo object not found");
				}
			}

			GUILayout.Label ("These buttons will get that color and size from Path Master");

			if (GUILayout.Button ("Start Point")) {
				myTarget.color = myTarget.myMaster.startColor;
				myTarget.size = myTarget.myMaster.startSize;
				SceneView.RepaintAll ();
			}

			if (GUILayout.Button ("Mid Point")) {
				myTarget.color = myTarget.myMaster.midColor;
				myTarget.size = myTarget.myMaster.midSize;
				SceneView.RepaintAll ();
			}

			if (GUILayout.Button ("End Point")) {
				myTarget.color = myTarget.myMaster.endColor;
				myTarget.size = myTarget.myMaster.endSize;
				SceneView.RepaintAll ();
			}

			if (GUILayout.Button ("Special Point")) {
				myTarget.color = Color.cyan;
				myTarget.size = 0.4f;
				SceneView.RepaintAll ();
			}
		}
	}
}
