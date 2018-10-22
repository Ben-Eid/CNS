using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Account : MonoBehaviour {
	public int games, wins, rating;
	public string displayName, email;
	public string password;
	public bool isConnected;
	
	public Account(string nName, string nEmail, string nPassword){
		displayName = nName;
		email = nEmail;
		password = nPassword;
	}
	
	public void CompleteGame(bool won){
		games++;
		wins += (won)?1:0;
	}
}