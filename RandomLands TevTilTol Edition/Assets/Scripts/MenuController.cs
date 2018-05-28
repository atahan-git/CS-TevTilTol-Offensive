using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {

	public static MenuController s;

    public int menuLevelId = 3;
	public int playLevelId = 2;
	public int schoolLevelId = 1;
    //public int tutorialLevelId = 3;
    //public Animator tutorialAnim;

    //public TeleporterStatus status;

    //public GameObject particleEffects;
    //public Transform effectsTransform;

    //int defQualityLevel;

    //public UnityEngine.UI.Text options;

    public GameObject menu;
    public GameObject optionsMenu;
	public GameObject hostMenu;
	public GameObject normalStartMenu;
	public GameObject quickStart;
	public GameObject tutorialMenu;
	public GameObject creditsMenu;
	public Text t_quickStart;
    public GameObject mainScreen;
    public float transitionSpeed = 1;
	public GameObject extraCredits;

	void Awake () {
		s = this;
	}

	// Use this for initialization
	void Start () {
		if(SceneManager.sceneCount == 1)
			SceneManager.LoadScene(schoolLevelId, LoadSceneMode.Additive);

		//s = this;

		Time.timeScale = 1f;
		Time.fixedDeltaTime = Time.timeScale * 0.02f;

        //defQualityLevel = QualitySettings.GetQualityLevel();
        QualitySettings.SetQualityLevel(5, true);
		//PlayerPrefs.SetInt ("Quality", defQualityLevel);

		if (PlayerPrefs.GetInt ("Diff", -1) != -1) {
			quickStart.SetActive (true);
			if (t_quickStart) {
				switch (PlayerPrefs.GetInt ("Diff", -1)) {
				case 0:
					t_quickStart.text = (PlayerPrefs.GetInt ("Lang", 0) == 0) ? "Easy" : "Kolay";
					break;
				case 1:
					t_quickStart.text = (PlayerPrefs.GetInt ("Lang", 0) == 0) ? "Normal/Kind" : "Normal/Yumuşak";
					break;
				case 2:
					t_quickStart.text = (PlayerPrefs.GetInt ("Lang", 0) == 0) ? "Normal" : "Normal";
					break;
				case 3:
					t_quickStart.text = (PlayerPrefs.GetInt ("Lang", 0) == 0) ? "Normal/Harsh" : "Normal/Zorlu";
					break;
				case 4:
					t_quickStart.text = (PlayerPrefs.GetInt ("Lang", 0) == 0) ? "Hardcore" : "Hardcore";
					break;
				default:
					quickStart.SetActive (false);
					break;
				}
			}
		} else {
			quickStart.SetActive (false);
		}
		//print ("this called");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	bool isStart = false;

    public void Play()
    {
		if (isStart == false) {
			isStart = true;
			CommenceLevelLoad ();
			//SceneManager.LoadSceneAsync (playLevelId - 1, LoadSceneMode.Additive);
			RandomLevelLoader.s.LoadScene();
			//GameObject.FindObjectOfType<HasretDisabler> ().DisableThatShittyBuilding (true);
			SceneManager.UnloadScene (menuLevelId);
		}
    }

	public void PlayExplore()
	{
		if (isStart == false) {
			isStart = true;
			CommenceLevelLoad ();
			//SceneManager.LoadSceneAsync (playLevelId - 1, LoadSceneMode.Additive);
			RandomLevelLoader.s.EngageExploration();
			//GameObject.FindObjectOfType<HasretDisabler> ().DisableThatShittyBuilding (true);
			SceneManager.UnloadScene (menuLevelId);
		}
	}

	public void PlayDiffChange(int difficulty)
	{
		if (isStart == false) {
			isStart = true;
			PlayerPrefs.SetInt ("Diff", difficulty);

			CommenceLevelLoad();
			RandomLevelLoader.s.LoadScene();
			SceneManager.UnloadScene (menuLevelId);
			//SceneManager.LoadSceneAsync(playLevelId - 1);
			//SceneManager.LoadSceneAsync(playLevelId, LoadSceneMode.Additive);
		}
	}
		

    void CommenceLevelLoad()
    {
        //status.OpenLamp();
        //Instantiate(particleEffects, effectsTransform.position, effectsTransform.rotation);
        //QualitySettings.SetQualityLevel(defQualityLevel, true);
    }

    GameObject from;
    GameObject to;

    public void Options()
    {
        from = menu;
        to = optionsMenu;
        noTransition ();
    }

    public void OptionsBack()
    {
        from = optionsMenu;
        to = menu;
        noTransition ();
    }

	public void Tutorial()
	{
		from = menu;
		to = tutorialMenu;
		noTransition ();
	}

	public void TutorialBack()
	{
		from = tutorialMenu;
		to = menu;
		noTransition ();
	}

	public void Credits()
	{
		from = menu;
		to = creditsMenu;
		noTransition ();
	}

	public void CreditsBack()
	{
		from = creditsMenu;
		to = menu;
		noTransition ();
	}

	public void ExtraCredits()
	{
		from = creditsMenu;
		to = extraCredits;
		noTransition ();
	}

	public void ExtraCreditsBack()
	{
		from = extraCredits;
		to = creditsMenu;
		noTransition ();
	}


	public void ReloadOptions () {
		optionsMenu.SetActive (false);
		Invoke ("_reload", 0.1f);
	}

	void _reload () {
		optionsMenu.SetActive (true);
	}

	//----------------------------------------------------

	public void HostMenu () {

		from = menu;
		to = hostMenu;
		noTransition ();

	}
		

	public void StartMenuBack () {

		from = hostMenu;
		to = menu;
		noTransition ();
	}

	//----------------------------------------------------

    public void Exit()
    {
        Application.Quit();
    }

    void OnLevelWasLoaded(int level)
    {
        //QualitySettings.SetQualityLevel(defQualityLevel, true);
    }

    IEnumerator Transition ()
    {
        /*RectTransform fRect = mainScreen.GetComponent<RectTransform>();

        float fromStarty = fRect.localScale.y;
        float fromStartx = fRect.localScale.x;
        while (fRect.localScale.y > 0.0001f)
        {
            fRect.localScale = new Vector3(Mathf.Lerp(fRect.localScale.x, 100f, transitionSpeed * Time.deltaTime), Mathf.Lerp(fRect.localScale.y, 0, transitionSpeed * Time.deltaTime), fRect.localScale.z);
            yield return 0;
        }

        fRect.localScale = new Vector3(fromStartx, fromStarty, fRect.localScale.z);*/

		noTransition ();
		yield return 0;


    }

	void noTransition () {
		from.SetActive(false);
		to.SetActive(true);
	}

	public IEnumerator LangChange ()
	{
		RectTransform fRect = mainScreen.GetComponent<RectTransform>();

		float fromStarty = fRect.localScale.y;
		float fromStartx = fRect.localScale.x;
		while (fRect.localScale.y > 0.0001f)
		{
			fRect.localScale = new Vector3(Mathf.Lerp(fRect.localScale.x, 0.1f, transitionSpeed * Time.deltaTime), Mathf.Lerp(fRect.localScale.y, 0, transitionSpeed * Time.deltaTime), fRect.localScale.z);
			yield return 0;
		}

		fRect.localScale = new Vector3(fromStartx, fromStarty, fRect.localScale.z);

		LangChanger.SetTextLang ();
	}
}
