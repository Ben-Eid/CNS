﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Net;
using System.IO;
using System.Text;
public class Server : MonoBehaviour {
	//look up ports
	public int port = 6321;
	private bool serverStarted;
	
	private List<ServerClient> clients, disconnectList;
	
	private List<Account> accounts;
	
	private TcpListener server;
	
	public void Init(){
		DontDestroyOnLoad(gameObject);
		clients = new List<ServerClient>();
		disconnectList = new List<ServerClient>();
		accounts = new List<Account>();
		try {
			server = new TcpListener(IPAddress.Any, port);
			server.Start();
			StartListening();
			serverStarted = true;
		} catch(Exception e) {
			Debug.Log("Socket error: " + e.Message);
		}
	}
	
	private void StartListening(){
		server.BeginAcceptTcpClient(AcceptTcpClient, server);
	}
	
	private void AcceptTcpClient(IAsyncResult ar){
		TcpListener listener = (TcpListener)ar.AsyncState;
		string allUsers = "SWHO|";
		foreach(ServerClient i in clients){
			allUsers += i.clientName + '|';
			Debug.Log(allUsers);
		}
		ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(ar));
		clients.Add(sc);
		StartListening();
		Broadcast(allUsers, clients[clients.Count-1]);
	}
	
	//send from server
	private void Broadcast(string data, List<ServerClient> cl){
		foreach(ServerClient sc in cl){
			try{
				StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
				writer.WriteLine(data);
				writer.Flush();
			} catch(Exception e) {
				Debug.Log("Write error: " + e.Message);
			}
		}
	}

	private void Broadcast(string data, ServerClient c){
		List<ServerClient> sc = new List<ServerClient> {c};
		Broadcast(data, sc);
	}
	
	private void Update(){
		if(!serverStarted)
			return;
		foreach (ServerClient c in clients){
			//is the clinet still connected?
			if(!IsConnected(c.tcp)){
				c.tcp.Close();
				disconnectList.Add(c);
				continue;
			} else {
				NetworkStream s = c.tcp.GetStream();
				if(s.DataAvailable){
					StreamReader reader = new StreamReader(s, true);
					String data = reader.ReadLine();
					if(data != null)
						OnIncomingData(c, data);
				}
			}
		}
		for(int i=0; i<disconnectList.Count-1; i++){
			//tell our player someone has disconnected
			clients.Remove(disconnectList[i]);
			disconnectList.RemoveAt(i);
		}
	}
	
	private bool IsConnected(TcpClient c){
		try{
			if(c != null && c.Client != null && c.Client.Connected){
				if(c.Client.Poll(0, SelectMode.SelectRead))
					return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
				return true;
			} else {
				return false;
			}
		} catch{
			return false;
		}
	}
	
	//read from server
	private void OnIncomingData(ServerClient c, string data){
		string[] aData = data.Split('|');
		Debug.Log(c.clientName + " Says: " + data);
		switch(aData[0]){
			case "cWHO":
				c.clientName = aData[1];
				c.isHost = (aData[2] == "0")? false:true;
				if(c.isHost){
					c.clientName = "host";
				}
				Broadcast("sCNN|" + c.clientName, clients);
				break;
			case "cSEL":
				Broadcast("sSEL|" + aData[1], clients[(c.isHost)?1:0]);
				break;
			case "cGAME":
				
				break;
			default:
				break;
		}
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
	
	private void DeObfuscate(string s){
		
	}
}

public class ServerClient {
	public string clientName;
	public TcpClient tcp;
	public bool isHost;
	public Account account;
	public ServerClient(TcpClient tcp){
		this.tcp = tcp;
	} 
}