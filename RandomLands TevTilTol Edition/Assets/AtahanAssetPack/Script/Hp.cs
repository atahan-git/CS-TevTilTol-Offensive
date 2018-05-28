using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Hp : NetworkBehaviour {

	[SyncVar]
	public int hpi = 20;
	public int maxhp = 20;
	public Slider healthbar;
    public Slider healthbar2;

	[SyncVar]
	public int mySide = -1;
	// Use this for initialization
	void Start () {
		hpi = maxhp;
		if(healthbar != null)
			healthbar.maxValue = maxhp;
        if (healthbar2 != null)
            healthbar2.maxValue = maxhp;
	}
	
	// Update is called once per frame
	void Update () {
		if(healthbar != null){
			healthbar.value = hpi;
		}
        if (healthbar2 != null)
        {
            healthbar2.value = hpi;
        }
	

		//print (gameObject.name + " - " +  hpi.ToString());
	}
		
	public void Damage (int damage){
		//SetDirtyBit (0xFFFFFFFF);
			CmdDamage (damage);
	}

	public void Damage (int damage, int attackSide){
		//SetDirtyBit (0xFFFFFFFF);
		if (attackSide != mySide) {
			CmdDamage (damage);
		}
	}


	[Command]
	void CmdDamage (int damage){
		hpi -= damage;
	}


}