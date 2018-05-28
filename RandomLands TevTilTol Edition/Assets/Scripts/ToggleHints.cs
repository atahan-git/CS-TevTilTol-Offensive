using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ToggleHints : MonoBehaviour {

    Toggle tog;

    public int playerState = 0;

    bool isFirstSet = false;
    
	// Use this for initialization
	void Start () {
        tog = GetComponent<Toggle>();
        tog.isOn = PlayerPrefs.GetInt("hints", 1) == 1 ? true : false;
        isFirstSet = true;

    }
	
	// Update is called once per frame
	void Update () {
        playerState = PlayerPrefs.GetInt("hints", 1);

    }

    public void setHints ()
    {
        if (!isFirstSet)
            return;
        PlayerPrefs.SetInt("hints", PlayerPrefs.GetInt("hints", 1) == 1 ? 0 : 1);
        tog.isOn = PlayerPrefs.GetInt("hints", 1) == 1 ? true : false;
    }
}
