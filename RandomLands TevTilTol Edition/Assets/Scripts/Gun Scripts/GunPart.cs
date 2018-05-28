using UnityEngine;
using System.Collections;

public class GunPart : MonoBehaviour {

	public int priority = 1; //its priority applying stats
	//barrel = 1
	//Scope = 2
	//stock = 3
	//grip = 4
	public bool isBaseStats = false; //is body


	//BASICLY
	//if it add values it will add those values to the gun's stats
	//if it is multiply it will multiply the gun's values by that
	//if isBaseStats it will just make the guns stats those values (body parts should use that)

	//----------------------------------
	//-----------Add Values-------------
	//----------------------------------

	public float maxAccuracy = 0f; //default and maximum accuracy
	public float decreaseAccuracy = 0f; //accuracy decreased per bullet
	public float increaseAccuracy = 0f; //accuracy increased per tick
	public float leastAccuracy = 0f; //least possible accuracy
	
	public int ammoCapacity = 0;
	public float reloadSeconds = 0f;
	
	public int damage = 0;
	public float fireRate = 0f;

	//----------------------------------
	//--------Multiply Values-----------
	//----------------------------------

	public float MULmaxAccuracy = 1f; //default and maximum accuracy
	public float MULdecreaseAccuracy = 1f; //accuracy decreased per bullet
	public float MULincreaseAccuracy = 1f; //accuracy increased per tick
	public float MULleastAccuracy = 1f; //least possible accuracy
	
	public float MULammoCapacity = 1f;
	public float MULreloadSeconds = 1f;
	
	public float MULdamage = 1f;
	public float MULfireRate = 1f;

	GunController myGunCont;
    GunDropEffect myDropEffect;

	// Use this for initialization
	void Start () {

		myGunCont = GunController.myGunCont;
		//this.enabled = false;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void SetStats(int ThePriority)
    {

        if (isBaseStats)
			return;
		if (ThePriority != priority)
			return;

        myGunCont = GunController.myGunCont;
        AddStats();
        MultiplyStats();
		//print ("anan zaaa");
	}

	void SetBaseStats (){
		if (!isBaseStats)
			return;


        myGunCont = GunController.myGunCont;
        TheSetStats();
        
		//print ("Base anan zaaa");
	}

	//--------------------
	//part that does stuff
	//--------------------

	void TheSetStats (){

		myGunCont.maxAccuracy = maxAccuracy;
		myGunCont.decreaseAccuracy = decreaseAccuracy;
		myGunCont.increaseAccuracy = increaseAccuracy;
		myGunCont.leastAccuracy = leastAccuracy;
		myGunCont.ammoCapacity = ammoCapacity;
		myGunCont.reloadSeconds = reloadSeconds;
		myGunCont.damage = damage;
		myGunCont.fireRate = fireRate;

		//print ("stats set");

	}

	void AddStats (){

		//float lel = myGunCont.maxAccuracy + maxAccuracy;
		//print ("add  " + myGunCont.maxAccuracy.ToString() + " + " + maxAccuracy.ToString() + " = " + lel.ToString());

		myGunCont.maxAccuracy += maxAccuracy;

		//print (myGunCont.maxAccuracy);

		myGunCont.decreaseAccuracy += decreaseAccuracy;
		myGunCont.increaseAccuracy += increaseAccuracy;
		myGunCont.leastAccuracy += leastAccuracy;
		myGunCont.ammoCapacity += ammoCapacity;
		myGunCont.reloadSeconds += reloadSeconds;
		myGunCont.damage += damage;
		myGunCont.fireRate += fireRate;

		//print ("stats added");


	}

	void MultiplyStats (){

		//float lel2 = myGunCont.maxAccuracy * MULmaxAccuracy;
		//float lel2 = (int)(myGunCont.damage * MULdamage);
		//print ("multiply " + myGunCont.maxAccuracy.ToString() + " * " + MULmaxAccuracy.ToString() + " = " + lel2.ToString());
		//print ("multiply " + myGunCont.damage.ToString() + " * " + MULdamage.ToString() + " = " + lel2.ToString());
		
		myGunCont.maxAccuracy *= MULmaxAccuracy;

		//print (myGunCont.maxAccuracy);

		myGunCont.decreaseAccuracy *= MULdecreaseAccuracy;
		myGunCont.increaseAccuracy *= MULincreaseAccuracy;
		myGunCont.leastAccuracy *= MULleastAccuracy;
		myGunCont.ammoCapacity = (int)(myGunCont.ammoCapacity * MULammoCapacity);
		myGunCont.reloadSeconds *= MULreloadSeconds;
		myGunCont.damage = (int)(myGunCont.damage * MULdamage);
		myGunCont.fireRate *= MULfireRate;

		//print ("stats multiplied");

	}

    void SetVisualStat(int ThePriority)
    {

        if (isBaseStats)
            return;
        if (ThePriority != priority)
            return;

        myDropEffect = transform.root.gameObject.GetComponentInChildren<GunDropEffect>();

        myDropEffect.damage += damage;
        myDropEffect.fireRate += fireRate;

        myDropEffect.damage = (int)(myDropEffect.damage * MULdamage);
        myDropEffect.fireRate *= MULfireRate;

        Transform[] myObjects = GetComponentsInChildren<Transform>();

        foreach(Transform obj in myObjects)
        {

            obj.gameObject.layer = 0;
        }
        //print ("anan zaaa");
    }

    void SetBaseVisualStat()
    {
        if (!isBaseStats)
            return;


        myDropEffect = transform.root.gameObject.GetComponentInChildren<GunDropEffect>();

        myDropEffect.damage = damage;
        myDropEffect.fireRate = fireRate;

        Transform[] myObjects = GetComponentsInChildren<Transform>();

        foreach (Transform obj in myObjects)
        {

            obj.gameObject.layer = 0;
        }
        //print ("Base anan zaaa");
    }

}
