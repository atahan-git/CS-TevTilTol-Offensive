using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Health : MonoBehaviour {

	public static Health s;

	float lerp = 4f;

	public int health = 100;
	public int maxHealth = 200;

	public Slider healthSlider;
	public Text maxHealthText;
	public Text healthText;

	public bool isAlive = true;

	public GameObject hitEffect;
	public Transform hitEffectParent;

	public Image lowHpEffect;
	public float lowHpPercent = 0.2f;

	float healthMultiplier = 1f;

	// Use this for initialization
	void Start () {
		s = this;
		//health = 100;

		maxHealth = (int)(maxHealth * healthMultiplier);
		health = maxHealth;
		//CalculateLevel();
	}

	float lerpFloat = 0;
	
	// Update is called once per frame
	void Update () {
		if (healthSlider != null) {
			healthSlider.maxValue = maxHealth;
			/*if (Mathf.Abs (healthSlider.value - health) > 4) {
				healthSlider.value = Mathf.Lerp (healthSlider.value, health, lerp);
			} else {
				healthSlider.value = health;
			}*/
			lerpFloat = Mathf.Lerp (lerpFloat, (float)health, lerp * Time.deltaTime);
			healthSlider.value = (int)lerpFloat;
			maxHealthText.text = " " + maxHealth;
			healthText.text = " " + health;
		}

		if (lowHpEffect != null) {
			if ((float)health < (float)maxHealth * (float)lowHpPercent) {

				float percent = 1f - ((float)health / ((float)maxHealth * (float)lowHpPercent));

				lowHpEffect.color = new Color (1, 1, 1, percent);

			} else {
				lowHpEffect.color = new Color (1, 1, 1, 0);
			}
		}

		//print (health);
		/*if (health <= 0) {
			AllDie ();
			isAlive = false;
		}*/

        /*if (Input.GetKeyDown(KeyCode.H))
        {
            health = maxHealth;
        }*/
	}

	public void CalculateLevel (){
		return;
		//print ("health Calculated");
		maxHealth = (int)(((float)XpController.xp.level * (float)XpController.xp.level / 2f + 20f) * 10f * healthMultiplier);

		if(health > 0)
			health = maxHealth;

	}

	public void Damage(int damage, Transform caller){
		Damage (damage,caller.position);
	}

	//use this to damage us
	public void Damage (int damage, Vector3 caller){
		//health -= damage;
		ScoreController.myScore.AddScore (-damage);

		GameObject myHit = (GameObject)Instantiate (hitEffect, hitEffectParent.position, hitEffectParent.rotation);
		myHit.GetComponent<RectTransform> ().SetParent (hitEffectParent, false);
		myHit.transform.localScale = new Vector3 (1, 1, 1);
		myHit.GetComponent<HitEffect> ().hitPos = caller;
		myHit.GetComponent<HitEffect> ().fadeTime = map (Mathf.Clamp (damage, 5, 50), 5, 50, 4, 1);
		myHit.GetComponent<HitEffect> ().damage = -damage;
	}

	public void AllDie (){
		//Debug.Log ("Death by Health Script is disabled");
		//BroadcastMessage ("Die");
		//Application.LoadLevel (0);
	}

	float map (float x, float in_min, float in_max, float out_min, float out_max){

		return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
	}
}

