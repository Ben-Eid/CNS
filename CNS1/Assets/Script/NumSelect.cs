using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumSelect : MonoBehaviour 
{
	public static NumSelect Instance {set; get;}
	public bool isHost;
	private Client client;
	public int myNum, opponentNum;
	
	private void Start(){
		Instance = this;
		client = FindObjectOfType<Client>();
		isHost = client.isHost;
	}
	
	private void Update(){
		if(myNum != 0 && opponentNum != 0){
			GetResults();
		}
	}
	
	private void GetResults(){
		int result = myNum - opponentNum;
		if(result == 0){
			EndGame("tie");
		} else if(result == 1 || result == -2){
			EndGame("Win");
		} else{
			EndGame("Loss");
		}		
	}
	
	private void EndGame(string msg){
		client.Send("cGame" + msg);
		myNum = 0;
		opponentNum = 0;
	}
	
	public void Select1(){
		myNum = 1;
		client.Send("cSEL|" + myNum);
	}
	
	public void Select2(){
		myNum = 2;
		client.Send("cSEL|" + myNum);
	}
	
	public void Select3(){
		myNum = 3;
		client.Send("cSEL|" + myNum);
	}
}