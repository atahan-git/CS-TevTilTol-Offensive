using UnityEngine;
using System.Collections;

public class TeleporterStatus : MonoBehaviour {

    public Color OpenColor;
    public Color CloseColor;
    public Color BlinkColor;

    public float blinkTime = 0.5f;

    public bool isOpen = false;

    Renderer rend;
    Material mat;
	// Use this for initialization
	void Start () {
        rend = GetComponent<Renderer>();

        mat = rend.material;
        mat.EnableKeyword("_EMISSION");

        CloseLamp();

        InvokeRepeating("Blink", blinkTime, blinkTime*2);

    }
	
	// Update is called once per frame
	void Update () {

        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isOpen)
            {
                isOpen = false;
                CloseLamp();
            }
            else
            {
                isOpen = true;
                OpenLamp();
            }
        }*/

	}

    public void Blink()
    {
        mat.SetColor("_EmissionColor", BlinkColor);
        Invoke("Blink2", blinkTime);
        //print("blink1");
    }

    public void Blink2()
    {
        mat.SetColor("_EmissionColor", CloseColor);
        //print("blink2");
    }

    public void OpenLamp()
    {
        CancelInvoke();
        mat.SetColor("_EmissionColor", OpenColor);

        AudioSource[] aud = GetComponentsInChildren<AudioSource>();

        foreach(AudioSource myAud in aud)
        {
            myAud.Play();
        }
        //print("opened");
    }

    public void CloseLamp()
    {
        //print("closed");
        mat.SetColor("_EmissionColor", CloseColor);
    }
}
