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
		Debug.Log(Obfuscate("apples"));
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
		char[] alpha = {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'};
		//selects a random english character
		int num = UnityEngine.Random.Range(0,25);
		char key = alpha[num];
		//the string that will be rwturned
		StringBuilder ret = new StringBuilder(s);
		int addedChars = 0;
		//adds some random characters to the end of the string to make sure it's long enough for the key to be in the right position
		while(ret.Length < num){
			ret.Append(alpha[UnityEngine.Random.Range(0,25)]);
			addedChars++;
		}
		//adds the key to it's proper place
		ret.Append(key);
		//adds some more random characters to mess with people
		for(int i=0; i<5 && i<num; i++){
			ret.Append(alpha[UnityEngine.Random.Range(0,25)]);
		}
		//adds the way to find the key at the front
		ret.Insert(0,  alpha[addedChars]);
		for(int i=1; i<num; i++){
			UnityEngine.Random.InitState(num);
			ret[i] = alpha[(Find(alpha, ret[i]) + UnityEngine.Random.Range(0,num)) % 26];
		}
		return ret.ToString();
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