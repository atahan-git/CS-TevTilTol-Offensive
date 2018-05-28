using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SensitivityMenuItem : MonoBehaviour {

    public Slider slider;
    public InputField input;

    float curSensitivity = -2f;

	// Use this for initialization
	void Awake () {
		slider = GetComponentInChildren<Slider> ();
		input = GetComponentInChildren<InputField> ();
        curSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
        //print("get float" + curSensitivity);
        UpdateValues();
    }
	
	// Update is called once per frame
	void Update () {
        //print(PlayerPrefs.GetFloat("MouseSensitivity", 2f));

    }   

    public void OnSliderChange()
    {
        if (curSensitivity < 0)
            return;
        curSensitivity = slider.value;
        UpdateValues();
    }

    public void OnTextChange()
    {
        if (curSensitivity < 0)
            return;
        string myText = input.text;

        float myNumber = 2f;

        try
        {
            myNumber = float.Parse(myText);
        }
        catch
        {
            input.text = 2.ToString();
        }

        myNumber = Mathf.Clamp(myNumber, 0f, 5f);

        curSensitivity = myNumber;

        UpdateValues();
    }

    void UpdateValues()
    {
        PlayerPrefs.SetFloat("MouseSensitivity", curSensitivity);
        //print("set float" + curSensitivity);
        slider.value = curSensitivity;
        input.text = curSensitivity.ToString();
    }
}
