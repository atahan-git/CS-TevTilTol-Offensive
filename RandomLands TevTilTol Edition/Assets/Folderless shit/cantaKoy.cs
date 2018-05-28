using UnityEngine;
using System.Collections;

public class cantaKoy : MonoBehaviour {
	
	public GameObject[] cantalar = new GameObject[] {};
	public int count;
	
	private int instantiated;
	private int index;
	
	public int minForce = -100;
	public int maxForce = 100;
	public int minTorque = 1000000;
	public int maxTorque = 10000000;
	
	
	void Start(){
		if(CantaKoyMaster.s.koymaliMi)
		StartCoroutine(create());
	}
	
	IEnumerator create() {
		yield return new WaitForSeconds (0.3f);
		while(instantiated < count){
			instantiated++;
			index = Random.Range(0, cantalar.Length);
			//Debug.Log(index);
			GameObject obj = (GameObject) Instantiate(cantalar[index], transform.position, Quaternion.identity);
			obj.GetComponent<Rigidbody>().AddForce(Random.Range(minForce, maxForce), Random.Range(minForce, maxForce), Random.Range(minForce, maxForce));
			obj.GetComponent<Rigidbody>().AddTorque(Random.Range(minTorque, maxTorque), Random.Range(minForce, maxForce), Random.Range(minForce, maxForce));
			//obj.transform.parent = transform;
            yield return new WaitForSeconds (0.3f);
			//Give random motion to instantiated GameObject
		}
	}
}