using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TextFadeOut : MonoBehaviour {

    Text myText;
    float fadeTime = 5f;
    float waitTime = 2f;
    float fadeinTime = 8f;
    public Color OpenColor;
    public Color CloseColor;

    bool isFaded = false;
    
	// Use this for initialization
	void Start () {
        myText = GetComponent<Text>();
        myText.color = CloseColor;
        //if (Application.isEditor)
            //return;
        if (PlayerPrefs.GetInt("hints", 1) == 0)
            return; 
        StartCoroutine("FadeIn");
        
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0) && !isFaded)
        {
            StopAllCoroutines();
            StartCoroutine("FadeOut");
            isFaded = true;
        }
    }

    IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(waitTime / 2f);
        while (myText.color.a < 0.9)
        {
            myText.color = Color.Lerp(myText.color, OpenColor, fadeinTime * Time.deltaTime);
            yield return null;
        }

        myText.color = OpenColor;
        
    }

    IEnumerator FadeOut()
    {

        while (myText.color.a > 0.1)
        {
            myText.color = Color.Lerp(myText.color, CloseColor, fadeTime * Time.deltaTime);
            yield return null;
        }

        myText.color = CloseColor;
    }
}
