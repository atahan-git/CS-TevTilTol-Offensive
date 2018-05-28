using UnityEngine;
using System.Collections;

public class UFO_Death : MonoBehaviour {

    Hp Hpin;

    public GameObject ExplosionPrefab;
    public GameObject BrokenSpaceShip;

    public GameObject Ufo;
    public GameObject[] toBeDisabled;
    GameObject brokenShip;

    bool isDeath = false;

    // Use this for initialization
    void Start()
    {
        Hpin = GetComponent<Hp>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Hpin.hpi <= 0 && !isDeath)
        {
            isDeath = true;
            GameObject.FindGameObjectWithTag("Player").GetComponent<DeathController>().Win();
            ScoreController.myScore.AddScore((int)(ScoreController.myScore.score/10));

            int explosionCount = Random.Range(4, 8);

            for(int i = 0; i <= explosionCount; i++)
            {
                Invoke("InstantiateExplosion", Random.Range(0.2f, 8f));
            }

            brokenShip = (GameObject)Instantiate(BrokenSpaceShip, Ufo.transform.position, Ufo.transform.rotation);
            Ufo.SetActive(false);

            foreach(GameObject disableThis in toBeDisabled)
            {
                disableThis.SetActive(false);
            }
        }

    }

    void InstantiateExplosion()
    {
		Instantiate(STORAGE_Explosions.s.bigExp, brokenShip.transform.position, brokenShip.transform.rotation);
    }

}
