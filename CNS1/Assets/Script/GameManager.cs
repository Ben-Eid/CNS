using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.Text;

public class GameManager : MonoBehaviour {
	public static GameManager Instance{ set; get;}
	
	public GameObject serverPrefab, clientPrefab;
	public GameObject mainMenu, serverMenu, connectMenu;
	
	public InputField nameInput;
	
	private void Start(){
		Instance = this;
		mainMenu.SetActive(true);
		serverMenu.SetActive(false);
		connectMenu.SetActive(false);
		DontDestroyOnLoad(gameObject);
	}
	
	public void BackButton(){
		serverMenu.SetActive(false);
		connectMenu.SetActive(false);
		mainMenu.SetActive(true);
		
		Server s = FindObjectOfType<Server>();
		if(s != null)
			Destroy(s.gameObject);
		
		Client c = FindObjectOfType<Client>();
		if(c != null)
			Destroy(c.gameObject);
	}
	
	public void HostButton(){
		try{
			Server s = Instantiate(serverPrefab).GetComponent<Server>();
			s.Init();
			Client c = Instantiate(clientPrefab).GetComponent<Client>();
			c.clientName = nameInput.text;
			if(c.clientName == ""){
				c.clientName = "host";
			}
			c.isHost = true;
			c.ConnectToServer("127.0.0.1", 6321);
			
		} catch(Exception e) {
			Debug.Log(e.Message);
		}
		
		mainMenu.SetActive(false);
		serverMenu.SetActive(true);
	}
	
	public void ConnectButton(){
		string testString = "fuckthedodgers";
		Debug.Log(DeObfuscate(Obfuscate(testString)));
		mainMenu.SetActive(false);
		connectMenu.SetActive(true);
	}
	
	public void ConnectToServerButton(){
		string hostAddress = GameObject.Find("HostInput").GetComponent<InputField>().text;
		if(hostAddress == ""){
			hostAddress = "127.0.0.1";
		}
		
		try{
			Client c = Instantiate(clientPrefab).GetComponent<Client>();
			c.clientName = nameInput.text;
			if(c.clientName == ""){
				c.clientName = "client";
			}
			c.ConnectToServer(hostAddress, 6321);
			connectMenu.SetActive(false);
		} catch(Exception e){
			Debug.Log(e.Message);
		}
	}
	
	public void StartGame(){
		SceneManager.LoadScene("Game");
	}
	
	private string Obfuscate(string s){
		StringBuilder sb = new StringBuilder();
		char[] alpha = {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'};
		//choose a random letter, this will be the seed.
		int obFirstIntKey = UnityEngine.Random.Range(0,25);
		char obFirstCharKey = alpha[obFirstIntKey];
		int a = 0;
		for(int i=0; i<s.Length; i++){
			UnityEngine.Random.InitState(obFirstIntKey + i);
			a = UnityEngine.Random.Range(0,25);
			sb.Append(alpha[(Find(alpha, s[i]) + a) % 26]);
		}
		//choose another random letter, this will be put at the start of the string to point to where the seed key will be
		int obSecondIntKey = UnityEngine.Random.Range(s.Length + 1,25);
		char obSecondCharKey = alpha[obSecondIntKey];
		//place the second key at the beginning of the string
		sb.Insert(0,obSecondCharKey);
		//add letters to make sure obFirstCharKey is in the right position and count how many new letters needed to be added
		int obAddedLetters = 0; 
		while(sb.Length < obSecondIntKey){
			obAddedLetters++;
			sb.Append(alpha[UnityEngine.Random.Range(0,25)]);
		}
		//place the character representation of the amount of added characters as the second char in the string.
		sb.Insert(1,alpha[obAddedLetters]);
		//places the firstcharkey at the position determined by the secondCharKey.
		sb.Append(obFirstCharKey);
		//add a bunch of random characters to the back. because these won't be used at all we do not need to keep track of them.
		int obEndCharacters = UnityEngine.Random.Range(0,20);
		for(int i=0; i<obEndCharacters; i++){
			sb.Append(alpha[UnityEngine.Random.Range(0,25)]);
		}
		
		//return the new string
		Debug.Log("Obfuscation produces: " + sb.ToString());
		return sb.ToString();
	}
	
	private string DeObfuscate(string s){
		StringBuilder sb = new StringBuilder(s);
		char[] alpha = {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'};
		//grab the char keys pointing to where the original key is, and the char detailing how many added characters there are.
		//the second char key will always be the first in the string
		char deSecondCharKey = s[0];
		int deSecondIntKey = Find(alpha, deSecondCharKey);
		int deAddedLetters = Find(alpha, s[1]);
		//remove the first two keys from the string
		sb.Remove(0,2);
		char deFirstCharKey = s[deSecondIntKey+1];
		int deFirstIntKey = Find(alpha, deFirstCharKey);
		int a = 0;
		sb.Remove((deSecondIntKey - deAddedLetters - 1), sb.Length + 1 - (deSecondIntKey - deAddedLetters));
		for(int i=0; i<sb.Length; i++){
			UnityEngine.Random.InitState(deFirstIntKey + i);
			a = Find(alpha, sb[i]) - UnityEngine.Random.Range(0,25);
			if(a < 0){
				a += 26;
			}
			sb[i] = alpha[a];
		}
		//return the new string
		return sb.ToString();
	}
	
	private int Find(char[] array, char toFind){
		for(int i=0; i<array.Length; i++){
			if(array[i] == toFind){
				return i;
			}
		}
		return -1;
	}
}