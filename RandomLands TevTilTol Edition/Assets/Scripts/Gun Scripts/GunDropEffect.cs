using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GunDropEffect : MonoBehaviour {


    public int damage = 1;
    public float fireRate = 600f; //bullets per minute BPM

    public Text[] DPS_Texts;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation = Quaternion.identity;
        UpdateHud();

    }

    void UpdateHud()
    {
        foreach(Text myText in DPS_Texts)
        {
			if (myText != null) {
				int DPS = (int)((float)damage * fireRate / 60);
				myText.text = DPS + " DPS";
			}
        }

    }
}
