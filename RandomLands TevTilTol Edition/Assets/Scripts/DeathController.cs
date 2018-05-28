using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DeathController : MonoBehaviour {

	public static DeathController s;

	public string[] deathMessages;
	public MonoBehaviour[] scriptsToDisable;

	public GameObject realCam;
	public GameObject gunCam;
    public GameObject UICam;
    public GameObject deathCam;
	public Transform deathCamPos;
    public Transform WinCamPos;


    public float lerp = 0.01f;
	public float timeSlowAmount = 0.1f;
    public float lerp2 = 0.01f;



    public bool areWeDead = false;
    public bool areWeWon = false;

    public GameObject deathHUD;
	public Text score;
	public Text HScore;
	public Text codeLose;
	public Text deathMessageText;

    public GameObject WinHUD;
    public Text scoreWin;
    public Text HScoreWin;
	public Text codeWin;

	public static bool isEnded = false;
    // Use this for initialization
    void Start () {
		s = this;
		Time.timeScale = 1;
		Time.fixedDeltaTime = Time.timeScale * 0.02f;
	
	}
		

	// Update is called once per frame
	void Update () {


		if (areWeDead) {
			deathCam.transform.position = Vector3.Lerp (deathCam.transform.position, deathCamPos.position, lerp * Time.deltaTime);
			deathCam.transform.LookAt (transform.position);
		}
        if (areWeWon)
        {
            deathCam.transform.position = Vector3.Lerp(deathCam.transform.position, WinCamPos.position, lerp2 * Time.deltaTime);
			//deathCam.transform.LookAt(UFO.transform.position);
        }

    }

	public bool isRoundDeath = false;

	public void Die (){
        if (areWeWon)
            return;

		GameObject.FindObjectOfType<V2_EnemySpawner> ().StopAllCoroutines ();

		realCam.transform.DetachChildren ();
		gunCam.transform.DetachChildren ();
		realCam.SetActive (false);
		gunCam.SetActive (false);
        UICam.SetActive(false);
        deathCam.SetActive (true);

		areWeDead = true;

		Time.timeScale = timeSlowAmount;
		Time.fixedDeltaTime = Time.timeScale * 0.02f;
		ScoreController.myScore.Die ();

		score.text = "Score: " + ScoreController.myScore.score.ToString();
		HScore.text = "High Score: " + PlayerPrefs.GetInt ("HScore", 0).ToString();
		codeLose.text = "Code: " + PlayerPrefs.GetInt ("HScoreSec", 0).ToString();
		if (!isRoundDeath) {
			deathMessageText.text = deathMessages [Random.Range (0, deathMessages.Length - 1)];
		} else {
			if (Random.Range (0, 2) == 1) {
				deathMessageText.text = "Tevitölians are dead";
			} else {
				deathMessageText.text = "You ran out of time";
			}
		}

		foreach (MonoBehaviour myScript in scriptsToDisable) {
			myScript.enabled = false;
		}


		Invoke ("EnableHUD", 0.4f);

		//StartCoroutine (StopTime());
	}

	IEnumerator StopTime () {
		while (Time.timeScale > 0.01f) {

			Time.timeScale -= 0.00007f;
			Time.fixedDeltaTime = Time.timeScale * 0.02f;



			yield return null;
		}

		Time.timeScale = 0;
		print (Time.timeScale);
	}

    public MonoBehaviour enemySpawner;

    public void Win()
    {
		if (areWeDead)
			return;

		GameObject.FindObjectOfType<V2_EnemySpawner> ().StopAllCoroutines ();
		
        realCam.transform.DetachChildren();
        gunCam.transform.DetachChildren();
        realCam.SetActive(false);
        gunCam.SetActive(false);
        UICam.SetActive(false);
        deathCam.SetActive(true);

        areWeWon = true;
		isEnded = true;
        enemySpawner.enabled = false;

        Time.timeScale = 0.5f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

		ScoreController.myScore.AddScore (100000);
		ScoreController.myScore.WinScore ();


        scoreWin.text = "Score: " + ScoreController.myScore.score.ToString();
        HScoreWin.text = "High Score: " + PlayerPrefs.GetInt("HScore", 0).ToString();
		codeWin.text = "Code: " + PlayerPrefs.GetInt ("HScoreSec", 0).ToString();

        foreach (MonoBehaviour myScript in scriptsToDisable)
        {
            myScript.enabled = false;
        }


        Invoke("EnableWinHUD", 3f);

    }

    void EnableHUD (){
		deathHUD.SetActive (true);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

    void EnableWinHUD()
    {
        WinHUD.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

	public GameObject uSureMate;

	int sceneNum = 1;

	public bool isLoading = false;

	public void Restart (bool insta){
		if (insta) {
			if (!isLoading) {
				UnityEngine.SceneManagement.SceneManager.LoadSceneAsync (0);
				isLoading = true;
			}
		} else {
			sceneNum = 1;
			uSureMate.SetActive (true);
		}
    }
	public void Menu(bool insta)
    {
		if (insta) {

			if (!isLoading) {
				UnityEngine.SceneManagement.SceneManager.LoadSceneAsync (0);
				isLoading = true;
			}
		} else {
			sceneNum = 0;
			uSureMate.SetActive (true);
		}
    }


	public void IamSure () {
		//uSureMate.SetActive (false);
		if (!isLoading) {
			UnityEngine.SceneManagement.SceneManager.LoadSceneAsync (0);
			isLoading = true;
		}
	}

	public void IamNotSure () {
		uSureMate.SetActive (false);
	}
}
