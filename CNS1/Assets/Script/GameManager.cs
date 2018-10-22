using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

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
}