using UnityEngine;
using System.Collections;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(DialogTreeMaster))]
public class DialogTreeMasterEditor : Editor {

	public override void OnInspectorGUI () {
		DrawDefaultInspector ();
		DialogTreeMaster myTarget = (DialogTreeMaster)target;


		if (GUILayout.Button ("Add Tree")) {
			GameObject myObject = (GameObject)Instantiate (myTarget.treePrefab, Vector3.zero, Quaternion.identity);
			myObject.name = myTarget.gameObject.name + " - Dialog Tree " + (myTarget.trees.Length).ToString();
			myObject.GetComponent<TreeMaster> ().myDialogMaster = myTarget;
			AddToArray (myTarget.trees, myObject.GetComponent<TreeMaster> (), myTarget);
		}


		EditorGUILayout.LabelField("Be careful with this button, There is no Undo!", EditorStyles.boldLabel);

		if (GUILayout.Button ("Remove All Trees")) {
			foreach (TreeMaster myPath in myTarget.trees) {
				if (myPath != null) {
					DestroyImmediate (myPath.gameObject);
				}
			}

			myTarget.trees = new TreeMaster[0];
		}


	}

	void AddToArray (TreeMaster[] myArray,TreeMaster toAdd, DialogTreeMaster myTarget) {
		//Debug.Log ("this called");
		TreeMaster[] oldArray = new TreeMaster[myArray.Length];
		myArray.CopyTo (oldArray, 0);

		myArray = new TreeMaster[myArray.Length + 1];

		oldArray.CopyTo (myArray,0);

		myArray [myArray.Length - 1] = toAdd;

		myTarget.trees = myArray;

	}
}
