using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HitEffect : MonoBehaviour {

	public float fadeTime;
	public Vector3 hitPos;
	Transform player;
	Transform cam;

	public int damage = 0;

	public Text text;
	public Image effect;
	float val = 1f;

	//public bool lel;

	// Use this for initialization
	void Start () {
		cam = Camera.main.transform;
		player = GameObject.FindGameObjectWithTag("Player").transform;
		Invoke ("selfDestruction", 2f);
		text.text = damage.ToString ();
	}
	
	// Update is called once per frame
	void Update () {

		/*print ("normal");
		print (player.position - hitPos.position);
		print ("weird");
		print (new Vector3 (player.position.x, 0, player.position.z) - new Vector3 (hitPos.position.x, 0, hitPos.position.z));
		print ("------");*/
		Vector3 lookVector = new Vector3 (0, 0, 0);

		if(cam)
		lookVector = new Vector3 (cam.position.x, 0, cam.position.z) - 
			new Vector3 (hitPos.x, 0, hitPos.z);

		Quaternion lookRotation = Quaternion.LookRotation (lookVector);



		//lookRotation = Quaternion.Inverse (lookRotation);

		lookRotation = Quaternion.Euler (0, map(lookRotation.eulerAngles.y, 0f, 360f, 360f, 0f), 0);

		lookRotation = Quaternion.Euler (0, lookRotation.eulerAngles.y + player.rotation.eulerAngles.y, 0);

		//print (lookRotation.eulerAngles);


		transform.localRotation = Quaternion.Euler (0, 0, lookRotation.eulerAngles.y);

		val = Mathf.Lerp (val, 0, fadeTime * Time.deltaTime);

		effect.color = new Color (1, 1, 1, val);
		text.color = new Color (1, 0, 0, val);
			
	}

	void selfDestruction (){
		Destroy (gameObject);
	}

	float map (float x, float in_min, float in_max, float out_min, float out_max){

		return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
	}
}
