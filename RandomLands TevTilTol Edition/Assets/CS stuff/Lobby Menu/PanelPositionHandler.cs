using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelPositionHandler : MonoBehaviour {

	public static PanelPositionHandler s;

	List<GameObject> _panels = new List<GameObject>(9);

	public List<GameObject> panels{
		get{
			return _panels;
			UpdatePositions ();
		}
		set{
			_panels = value;
			UpdatePositions ();
		}
	}

	public GameObject[] parents = new GameObject[3];

	void Awake (){
		s = this;
	}

	void Update(){
		UpdatePositions ();
	}
	
	void UpdatePositions (){
		for (int i = 0; i < _panels.Count; i++) {
			if (_panels [i] != null) {
				try{
				if (parents [(int)Mathf.Clamp ((Mathf.Floor (i / 3f)), 0, parents.Length)] != null) {
					_panels [i].transform.SetParent (parents [(int)Mathf.Floor (i / 3)].transform);
					_panels [i].transform.SetSiblingIndex (i % 3);
				}
				}catch{
					print ("wtf " + ((int)Mathf.Clamp ((Mathf.Floor (i / 3f)), 0, parents.Length)).ToString());
				}
			}
		}
	}
}
