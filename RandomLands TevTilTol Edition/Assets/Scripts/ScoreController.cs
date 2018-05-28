using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour {

	public int score = 0;
	public int secretScore = 0;

	public Text ScoreText;
	public Text HighScoreText;

    public bool isChecked = false;
    public Text uSureButton;

	static public ScoreController myScore;

	bool isDed = false;

	int myDiff = 3;
	// Use this for initialization
	void Start () {

		HighScoreText.text = "High Score: " + PlayerPrefs.GetInt ("HScore", 0).ToString();
		if (!myScore)
			myScore = this;
	    
		myDiff = PlayerPrefs.GetInt ("Diff", 3);

		UpdateScore ();
	}
	

	void UpdateScore () {
        if(ScoreText)
		    ScoreText.text = "Score: " + score;
	
	}

	public void Die (){

		if (score > PlayerPrefs.GetInt ("HScore", 0)) {
			isDed = true;
			PlayerPrefs.SetInt("HScore", score);
			PlayerPrefs.SetInt("HScoreSec", (int)(secretScore/2) + 1376);
		}

	}

    public void WinScore()
    {

        if (score > PlayerPrefs.GetInt("HScore", 0))
        {
			isDed = true;
            PlayerPrefs.SetInt("HScore", score);
			PlayerPrefs.SetInt("HScoreSec", (int)(secretScore/2) + 1376);
        }
    }

    public void AddScore (int toAdd){

		if (isDed)
			return;

		switch (myDiff) {
		case 0:
			toAdd = (int)(toAdd * 0.5f);
			break;
		case 1:
			toAdd = (int)(toAdd * 0.8f);
			break;
		case 2:
			toAdd = (int)(toAdd * 1f);
			break;
		case 3:
			toAdd = (int)(toAdd * 1.2f);
			break;
		case 4:
			toAdd = (int)(toAdd * 2f);
			break;
		default:
			toAdd = (int)(toAdd * 1f);
			break;
		}

		if(toAdd > 0)
			ScoreText.text = "Score: " + score + " +" + toAdd;
		else
			ScoreText.text = "Score: " + score + " " + toAdd;
		score += toAdd;
		secretScore += toAdd * 7;
		Invoke ("UpdateScore", 0.5f);

	}
    
    public void ResetScore()
    {
        if (isChecked)
        {
            PlayerPrefs.SetInt("HScore", 0);
            HighScoreText.text = "High Score: " + PlayerPrefs.GetInt("HScore", 0).ToString();
            //isChecked = false;
            uSureButton.text = "Done";
        }
        else
        {
            uSureButton.text = "Are You Sure?";
            isChecked = true;
        }

    }
}
