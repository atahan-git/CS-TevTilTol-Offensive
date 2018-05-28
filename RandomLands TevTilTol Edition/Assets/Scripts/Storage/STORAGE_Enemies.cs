using UnityEngine;
using System.Collections;

public class STORAGE_Enemies : MonoBehaviour {


	public static STORAGE_Enemies s;

	//[Header("These are enemy levels")]
	public NestedGameObject[] EnemyLevels;

	// Use this for initialization
	void Awake () {
		s = this;	
	}
	
	// Update is called once per frame
	void Update () {
	
	}



	[System.Serializable]
	public class NestedGameObject {
		//[Header("These are enemy sizes")]
		public NestedGameObject2[] EnemySizes;
	}

	[System.Serializable]
	public class NestedGameObject2 {
		//[Header("These are enemy enemies")]
		public GameObject[] Enemies;
	}
}
