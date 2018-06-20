using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MessageController : NetworkBehaviour {
	
	public GameObject Log;
	public GameObject InputFieldText;
	public GameObject InputField;
	public string message = "";
	public int chatTimer = 0;
	bool foundEverything = false;
	public PlayerMaster pm;	
	
	[SerializeField]
	bool chatEnabled = false;
	
	void Start(){
		chatEnabled = false;
		message = "";
		Log = (GameObject.Find("ChatLog"));
		InputFieldText = (GameObject.Find("Text"));
		InputField = (GameObject.Find("InputField"));
		pm = gameObject.GetComponent<PlayerMaster>();
		
		foundEverything = true;
	}
	
	[ClientRpc]
	void RpcRecieveMessage(string message){
		chatTimer += 1000;
		Log.SetActive(true);
		(Log.GetComponent(typeof(Text)) as Text).text += "\n" + message;
		message = "";
		(InputFieldText.GetComponent(typeof(Text)) as Text).text = "";
	}
	
	[Command]
	void CmdSendMessage(string message){
		RpcRecieveMessage(message);
	}
	
	int i = 0;
	
	void FixedUpdate(){
		if(i < 200){
			i++;
			return;
		}
		if(!foundEverything) return;
		if(!isLocalPlayer) return;
		InputField.SetActive(chatEnabled);
		if(chatTimer > 0){
			chatTimer--;
		}else{
			Log.SetActive(chatEnabled);
		}
		message = pm.PlayerName + ": " + InputFieldText.GetComponent<Text>().text;
		if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)){
			if(chatEnabled){
				CmdSendMessage(message);
				message = "";
				chatTimer += 100;
			}else{
				chatEnabled = true;
				chatTimer += 100;
			}
		}
		if(Input.GetKeyDown(KeyCode.Escape)){
			chatEnabled = false;
			chatTimer = 0;
		}
		if(chatTimer > 1200){
			chatTimer = 1200;
		}
	}
	
	public void OnMessageChanged(string message){
		this.message = message;
	}
}
