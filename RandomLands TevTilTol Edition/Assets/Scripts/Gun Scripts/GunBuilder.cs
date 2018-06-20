using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GunBuilder : NetworkBehaviour {

    public GameObject pickEffect;
    public bool isPlayerGun = false;
    public bool randomizeAtStart = false;
    GunController gcont;

	[SyncVar]
    public Gun myGun = new Gun();

    GameObject myBody;
    GameObject myBarrel;
    GameObject myMagazine;
    GameObject myScope;
    GameObject myStock;

    GameObject objBody;
    GameObject objBarrel;
    GameObject objMagazine;
    GameObject objScope;
    GameObject objStock;

    public Transform myBarrelPoint;

    public float gunSize = 1f;
    Vector3 size = new Vector3(1, 1, 1);
    Vector3 defSize = new Vector3(1, 1, 1);

    public bool shouldCollide = false;

    [Header("Debug Settings")]
    public bool keepRandomizing = false;
    public float randomizingSpeed = 0.1f;
    public bool isLogger = false;
    public Logger_GunStats log;
    public bool isCheck = false;
    public Vector2 stopIfnotInRange = new Vector2(50f, 150f);

	Transform parentTrans;

    // Use this for initialization
    void Start() {

		try {
			if (isLocalPlayer) {
					parentTrans = GetComponent<PlayerRelay> ().myPlayer.GetComponentInChildren<ShootAnimation> ().transform;
			} else {
				parentTrans = GetComponent<PlayerRelay>().GunParent.transform;

			}
		} catch {
			parentTrans = transform;
		}

        if (randomizeAtStart)
            RandomizeGunParts();

        size = new Vector3(gunSize, gunSize, gunSize);
        Invoke("BuildGun", 0.1f);


        gcont = GetComponent<GunController>();

        //print (size);
        //print (Resources.Load("Body/Body_" + myGun.bodyType, typeof(GameObject)));

    }

	public void BuildGun() {
		//BuildGun (true, false, shouldCollide);
		try {
			if (isLocalPlayer) {
				BuildGun (true, false, shouldCollide);
				CmdBuildVisualEnemyGun (myGun, false);
			} else if (!GetComponent<GunDrop> ()) {
				BuildGun (false, true, true);
			} else {
				CmdBuildVisualEnemyGun (myGun, true);
			}
		} catch {
		}
	}

	[Command]
	void CmdBuildVisualEnemyGun (Gun _gun, bool isEffect){
		/*if (!isLocalPlayer) {
			myGun = _gun;
			//BuildGun (false,true, true);
		}*/

		RpcBuildVisualEnemyGun (_gun, isEffect);
	}

	[ClientRpc]
	void RpcBuildVisualEnemyGun (Gun _gun, bool isEffect){
		if (!isLocalPlayer) {
			myGun = _gun;
			BuildGun (false, true, true);
		}

		if (isEffect) {
			PickEffect ();
		}
	}

    // Update is called once per frame
	public void BuildGun(bool isSetStats, bool isDefLayer,bool _shouldCollide) {
		print ("building gun");
        //destroy every previous part
        if (objBody != null) {
            //print ("test");
            Destroy(objBody);
            Destroy(objBarrel);
            Destroy(objScope);
            Destroy(objStock);
            Destroy(objMagazine);
        }

        if (keepRandomizing) {
            Invoke("Start", randomizingSpeed);
            RandomizeGunParts();
            //SetGunParts ();
        }

        //if havent set yet set parts
        //if (myBody == null) {
        SetGunParts();
        //}

        //We must find the correct prefab to spawn!
        /*myBody = (GameObject)Resources.Load("Body/Body_" + myGun.bodyType, typeof(GameObject));
		myBarrel = (GameObject)Resources.Load("Barrel/Barrel_" + myGun.barrelType, typeof(GameObject));
		myScope = (GameObject)Resources.Load("Scope/Scope_" + myGun.scopeType, typeof(GameObject));
		myStock = (GameObject)Resources.Load("Stock/Stock_" + myGun.stockType, typeof(GameObject));
		myGrip = (GameObject)Resources.Load("Grip/Grip_" + myGun.gripType, typeof(GameObject));*/
        //print (myBody);


        //if there is no body then there is no gun!
        if (myBody == null) {
            print("such body does not exist!");
            //return;
        }
        //spawn body first
		objBody = (GameObject)Instantiate(myBody, parentTrans.position, parentTrans.rotation);//spawn the part
		objBody.transform.parent = parentTrans;//set parent
        objBody.transform.localScale = size;
        //objBody.transform.localScale = new Vector3 (5, 5, 5);
        //print ("that Called");

        //for position purposes
        Transform spawnPos;

        if (myBarrel != null) {
            //find where we are going to spawn the part
            spawnPos = (Transform)objBody.transform.Find("Barrel");
            objBarrel = (GameObject)Instantiate(myBarrel, spawnPos.position, spawnPos.rotation); //spawn the part
            objBarrel.transform.parent = spawnPos;//set parent
            objBarrel.transform.localScale = defSize;

            //set barrelPoint for shooting puposes
            myBarrelPoint = objBarrel.transform.Find("BarrelPoint");
        }

        if (myScope != null) {
            //find where we are going to spawn the part
            spawnPos = (Transform)objBody.transform.Find("Scope");
            objScope = (GameObject)Instantiate(myScope, spawnPos.position, spawnPos.rotation);//spawn the part
            objScope.transform.parent = spawnPos;//set parent
            objScope.transform.localScale = defSize;
        }

        if (myStock != null) {
            //find where we are going to spawn the part
            spawnPos = (Transform)objBody.transform.Find("Stock");
            objStock = (GameObject)Instantiate(myStock, spawnPos.position, spawnPos.rotation);//spawn the part
            objStock.transform.parent = spawnPos;//set parent
            objStock.transform.localScale = defSize;
        }

        if (myMagazine != null) {
            //find where we are going to spawn the part
            spawnPos = (Transform)objBody.transform.Find("Magazine");
            objMagazine = (GameObject)Instantiate(myMagazine, spawnPos.position, spawnPos.rotation);//spawn the part
            objMagazine.transform.parent = spawnPos;//set parent
            objMagazine.transform.localScale = defSize;
        }

		SetAllCollidersStatus(_shouldCollide);

		if (GetComponent<GunController>() && isSetStats)
		{
            //yield return 0;
            Invoke("SetStatsBroadcast", 0.1f);
        }
		else if (GetComponentInChildren<GunDropEffect>() || isDefLayer)
        {

            Invoke("SetVisualEffect", 0.1f);
        }
        //SetStatsBroadcast ();

		if (GetComponent<ConvincingPhysicsSync> ()) {
			GetComponent<ConvincingPhysicsSync> ().SetUpGunColliders (objBody);
		}
    }

    public void SetStatsBroadcast() {

        //print ("-----------------------------------");
        //print("----------CHANGING STATS-----------");
        //print ("-----------------------------------");

        GunPart[] myParts = GetComponentsInChildren<GunPart>();

        //foreach (GunPart part in myParts) {
		if (objBody != null) {
			objBody.BroadcastMessage ("SetBaseStats");
			objBody.BroadcastMessage ("SetStats", 1);
			objBody.BroadcastMessage ("SetStats", 2);
			objBody.BroadcastMessage ("SetStats", 3);
			objBody.BroadcastMessage ("SetStats", 4);
			print ("Set base stats: " + gameObject.name + " - " + objBody.name);
		}
        //}



        if (isPlayerGun) {
			gcont.ammoCapacity = (int)(gcont.ammoCapacity * gcont.ammoMultiplier);
			gcont.curAmmo = Mathf.Clamp(gcont.curAmmo, 0, gcont.ammoCapacity);
        }

        //print ("/////////////////////END\\\\\\\\\\\\\\\\\\\\\\");

        if (isLogger) {
            GunController myGunCont = GetComponent<GunController>();
            float data = (float)myGunCont.damage * (float)myGunCont.fireRate / 60f;
            log.AddData(data);
        }

        if (isCheck) {
            GunController myGunCont = GetComponent<GunController>();
            float data = (float)myGunCont.damage * (float)myGunCont.fireRate / 60f;
            if (data > stopIfnotInRange.y || data < stopIfnotInRange.x)
                Debug.LogError(data);
        }

		if (isLocalPlayer)
			CmdSetDps ();
    }

	[Command]
	void CmdSetDps (){
		GunController myGunCont = GetComponent<GunController>();
		ScoreBoard.s.dps [PlayerRelay.localRelay.myId] = (int)((float)myGunCont.damage * myGunCont.fireRate / 60f);
	}

    void SetVisualEffect()
    {
        GunPart[] myParts = GetComponentsInChildren<GunPart>();

        //foreach (GunPart part in myParts) {
		if (objBody != null) {
			objBody.BroadcastMessage ("SetBaseVisualStat");
			objBody.BroadcastMessage ("SetVisualStat", 1);
			objBody.BroadcastMessage ("SetVisualStat", 2);
			objBody.BroadcastMessage ("SetVisualStat", 3);
			objBody.BroadcastMessage ("SetVisualStat", 4);
		}
        //}

    }

    public void SetAllCollidersStatus(bool active) {
		foreach (Collider c in parentTrans.GetComponentsInChildren<Collider>()) {
            c.enabled = active;
            if (c.GetComponent<MeshCollider>())
                c.GetComponent<MeshCollider>().convex = true;

			c.enabled = active;
			//c.gameObject.name = active.ToString ();
        }
    }

    public void SetGunParts() {

        SetPart(out myBody, myGun.bodyLevel, myGun.bodyType, STORAGE_GunParts.s.body);
        SetPart(out myBarrel, myGun.barrelLevel, myGun.barrelType, STORAGE_GunParts.s.barrel);
        SetPart(out myMagazine, myGun.magazineLevel, myGun.magazineType, STORAGE_GunParts.s.magazine);
        SetPart(out myScope, myGun.scopeLevel, myGun.scopeType, STORAGE_GunParts.s.scope);
        SetPart(out myStock, myGun.stockLevel, myGun.stockType, STORAGE_GunParts.s.stock);

    }

    public void RandomizeGunParts() {

        Randomize(out myGun.bodyLevel, out myGun.bodyType, STORAGE_GunParts.s.body);
        Randomize(out myGun.barrelLevel, out myGun.barrelType, STORAGE_GunParts.s.barrel);
        Randomize(out myGun.magazineLevel, out myGun.magazineType, STORAGE_GunParts.s.magazine);
        Randomize(out myGun.scopeLevel, out myGun.scopeType, STORAGE_GunParts.s.scope);
        Randomize(out myGun.stockLevel, out myGun.stockType, STORAGE_GunParts.s.stock);

        //print ("RANDOMIZATION ENDED");

    }

    void Randomize(out int level, out int type, STORAGE_GunParts.NestedGameObject[] allParts) {

        if (allParts.Length > myGun.gunLevel) {
            if (allParts.Length > myGun.gunRarity)
                level = (int)Random.Range(myGun.gunRarity, myGun.gunLevel);
            else
                level = myGun.gunLevel;
        }
        else {
            if (allParts.Length > myGun.gunRarity)
                level = (int)Random.Range(myGun.gunRarity, allParts.Length);
            else
                level = allParts.Length - 1;
        }


        type = (int)Random.Range(0f, (float)allParts[level].GunParts.Length);
    }

    void SetPart(out GameObject part, int level, int type, STORAGE_GunParts.NestedGameObject[] allParts) {

        STORAGE_GunParts.NestedGameObject temp = allParts[level];
        //Debug.Log (type + " - " + temp.GunParts.Length + " - " + level);
        part = temp.GunParts[type];
    }

    [System.Serializable]
    public class Gun {

        public int gunLevel = 1;
        public int gunRarity = 0;


        public int bodyLevel = 0;
        public int bodyType = 0;

        public int barrelLevel = 0;
        public int barrelType = 0;

        public int magazineLevel = 0;
        public int magazineType = 0;

        public int scopeLevel = 0;
        public int scopeType = 0;

        public int stockLevel = 0;
        public int stockType = 0;

    }

	public void PickEffect (){
		Instantiate (pickEffect, transform.position, transform.rotation);
	}

	public void ChangeKeepRandomising (bool value){

		keepRandomizing = value;
		if (value)
			Start ();
	}
}
