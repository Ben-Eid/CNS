using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System;

public class Client : MonoBehaviour {
	public string clientName;
	
	public bool isHost, socketReady;
	
	private TcpClient socket;
	
	private NetworkStream stream;
	
	private StreamWriter writer;
	private StreamReader reader;
	
	private List<GameClient> players;
	
	//ensures that the client is not destroyed when switching scenes;
	private void Start(){
		players = new List<GameClient>();
		DontDestroyOnLoad(gameObject);
	}
	
	private void Update(){
		if(socketReady){
			if(stream.DataAvailable){
				string data = reader.ReadLine();
				if(data != null)
					OnIncomingData(data);
			}
		}
	}
	
	//read messages from the server
	private void OnIncomingData(string data){
		string[] aData = data.Split('|');
		Debug.Log("Server Says: " + data);
		switch(aData[0]){
			case "SWHO":
				for(int i=1; i<aData.Length-1; i++){
					UserConnected(aData[i], false);
				}
				Send("cWHO|" + clientName + '|' + ((isHost)?1:0).ToString());
				break;
			case "sCNN":
				UserConnected(aData[1], false);
				break;
			case "sSEL":
				NumSelect.Instance.opponentNum = Int32.Parse(aData[1]);
				break;
			default:
				break;
		}
	}
	
	//send messages to the server
	public void Send(string data){
		if(!socketReady)
			return;
		writer.WriteLine(data);
		writer.Flush();
	}
	
	public bool ConnectToServer(string host, int port){
		if(socketReady)
			return false;
		try{
			socket = new TcpClient(host, port);
			stream = socket.GetStream();
			writer = new StreamWriter(stream);
			reader = new StreamReader(stream);
			
			socketReady = true;
		} catch(Exception e) {
			Debug.Log("Socket error: " + e.Message);
		}
		return socketReady;
	}
	
	private void UserConnected(string name, bool isHost){
		GameClient c = new GameClient();
		c.name = name;
		players.Add(c);
		
		if(players.Count == 2){
			GameManager.Instance.StartGame();
		}
	}
	
	private void OnApplicationQuit(){
		CloseSocket();
	}
	
	private void OnDisable(){
		//CloseSocket();
	}
	
	private void CloseSocket(){
		if(!socketReady)
			return;
		writer.Close();
		reader.Close();
		socket.Close();
		socketReady = false;
	}
}
public class GameClient {
	public string name;
	public bool isHost;
}