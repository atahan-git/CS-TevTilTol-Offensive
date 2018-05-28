using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class XpController : MonoBehaviour {

	float lerp = 0.1f;

	public int curXp = 0;
	public int levelUpXp = 100;
	public int level = 1;

	static public XpController xp;

	public Slider xpSlider;
	public Text maxXpText;
	public Text xpText;
	public Text levelText;

	bool isLevelingUp = false;

	public AudioClip levelUpSound;
	public AudioSource audio;

	public GameObject levelUpPrefab;
	public Transform levelUpParent;

	// Use this for initialization
	void Start () {
	
		xp = this;
	}
	
	// Update is called once per frame
	void Update () {

		if (curXp >= levelUpXp) {
			LevelUp();
		}

		if (xpSlider != null && !isLevelingUp) {
			xpSlider.maxValue = levelUpXp;
			xpSlider.value = Mathf.Lerp(xpSlider.value, curXp, lerp);
			maxXpText.text = " " + levelUpXp;
			xpText.text = " " + curXp.ToString();
			levelText.text = "Level: " + level;
		}
	
	}

	void LevelUp (){
		audio.PlayOneShot (levelUpSound);
		isLevelingUp = true;
		ScoreController.myScore.AddScore ((int)(levelUpXp * Random.Range(0.8f, 0.6f)));
		curXp = curXp - levelUpXp;
		level ++;
		levelText.text = "LEVEL UP";
		levelText.fontSize = 23;
		Invoke ("NotLevelingUp", 1f);
		BroadcastMessage("CalculateLevel");

		GameObject lvUp = (GameObject)Instantiate (levelUpPrefab, levelUpParent.position, levelUpParent.rotation);
		lvUp.transform.SetParent(levelUpParent);
		lvUp.transform.localScale = new Vector3 (1, 1, 1);
	}

	void NotLevelingUp (){
		levelText.fontSize = 14;
		isLevelingUp = false;
	}

	public void CalculateLevel (){
		//print ("xp calculated");
		levelUpXp = (int)(((float)level * (float)level / 2f + Mathf.Pow((15f / 14f), (float)level) + 5f) * 50f);
	}
}
