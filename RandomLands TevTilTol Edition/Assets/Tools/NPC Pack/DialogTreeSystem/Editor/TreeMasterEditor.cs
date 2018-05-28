using UnityEngine;
using System.Collections;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(TreeMaster))]
public class TreeMasterEditor : Editor {

	public override void OnInspectorGUI () {
		DrawDefaultInspector ();
		TreeMaster myTarget = (TreeMaster)target;


		if (GUILayout.Button ("Add Dialog")) {
			GameObject myPoint = (GameObject)Instantiate (myTarget.dialogPrefab, myTarget.transform.position, myTarget.transform.rotation);
			myPoint.transform.parent = myTarget.transform;
		}


	}

}
