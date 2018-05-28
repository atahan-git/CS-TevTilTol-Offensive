using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroSelectPanel : MonoBehaviour {

	public int myId = -1;

	public Image myImg;

	public GameObject tooltip;
	public Text tooltipText;

	// Use this for initialization
	public void SetUp () {
		myImg.sprite = STORAGE_CharacterPrefabs.s.heroIcons [myId];	
		tooltipText.text = STORAGE_CharacterPrefabs.s.heroTooltips [myId];
		OnMouseExit ();
	}

	void OnDisable (){
		OnMouseExit ();
	}
	
	// Update is called once per frame
	public void OnMouseEnter () {
		if (!enabled)
			return;
		myImg.color = new Color (1,1,1,1);

		Invoke ("EnableTooltip", 0.4f);
	}

	void EnableTooltip (){
		tooltip.transform.localScale = new Vector3 (0,0,0);
		tooltip.SetActive (true);
		StartCoroutine (TooltipAnim());
	}

	IEnumerator TooltipAnim (){
		float scale = tooltip.transform.localScale.x;

		yield return null;
		while (1 - scale > 0.01f) {
			tooltip.transform.localScale = new Vector3 (scale, scale, scale);
			scale = Mathf.Lerp (scale, 1, 25f * Time.deltaTime);
			yield return null;
		}
		tooltip.transform.localScale = new Vector3 (1,1,1);
	}

	void DisableTooltip (){
		tooltip.SetActive (false);
	}

	public void OnMouseExit () {
		myImg.color = new Color (0.9f, 0.9f, 0.9f, 1);
		CancelInvoke ();
		StopAllCoroutines ();
		DisableTooltip ();  
	}

	public void SelectThis (){
		LobyPlayerController.localPlayer.ChangeHero (myId);
	}
}
