using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STORAGE_CharacterPrefabs : MonoBehaviour
{
	public static STORAGE_CharacterPrefabs s;

	public GameObject goMark;
	public GameObject attackMark;

	public GameObject[] heroes = new GameObject[4];
	public Sprite[] heroIcons = new Sprite[4];
	[TextArea]
	public string[] heroTooltips = new string[4];

	void Awake()
    {
		s = this;
	}
}
