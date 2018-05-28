using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(LeScript))]
public class LeScriptEditor : Editor {

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		LeScript myScript = (LeScript)target;
		if(GUILayout.Button("Randomize bagses"))
		{
			Debug.Log ("Do Stuff Here");
		}
	}
}
