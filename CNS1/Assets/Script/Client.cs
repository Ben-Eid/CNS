using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System;
using System.Text;

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
		string[] aData = DeObfuscate(data).Split('|');
		Debug.Log("Server Says: " + DeObfuscate(data));
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
		writer.WriteLine(Obfuscate(data));
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
	
	private static string Obfuscate(char[] alpha, string s){
		if(s.Length + 2 > alpha.Length){
			return "!!!BAD DATA!!!";
		}
		System.Random grabRandomInt = new System.Random();
		StringBuilder sb = new StringBuilder();
		//choose a random letter, this will be the seed.
		int obFirstIntKey = grabRandomInt.Next(0,alpha.Length - 1);
		char obFirstCharKey = alpha[obFirstIntKey];
		for(int i=0; i<s.Length; i++){
			grabRandomInt = new System.Random(obFirstIntKey + i);
			sb.Append(alpha[(FindChar(alpha, s[i]) + grabRandomInt.Next(0,alpha.Length - 1)) % alpha.Length]);
		}
		//choose another random letter, this will be put at the start of the string to point to where the seed key will be
		int obSecondIntKey = grabRandomInt.Next(s.Length + 1,alpha.Length);
		char obSecondCharKey = alpha[obSecondIntKey];
		//place the second key at the beginning of the string
		sb.Insert(0,obSecondCharKey);
		//add letters to make sure obFirstCharKey is in the right position and count how many new letters needed to be added
		int obAddedLetters = 0; 
		while(sb.Length < obSecondIntKey){
			obAddedLetters++;
			sb.Append(alpha[grabRandomInt.Next(0,alpha.Length - 1)]);
		}
		//place the character representation of the amount of added characters as the second char in the string.
		sb.Insert(1,alpha[obAddedLetters]);
		//places the firstcharkey at the position determined by the secondCharKey.
		sb.Append(obFirstCharKey);
		//add a bunch of random characters to the back. because these won't be used at all we do not need to keep track of them.
		int obEndCharacters = grabRandomInt.Next(0,20);
		for(int i=0; i<obEndCharacters && sb.Length + 2 < alpha.Length; i++){
			sb.Append(alpha[grabRandomInt.Next(0,alpha.Length - 1)]);
		}
		
		//return the new string
		return sb.ToString();
	}
	
	private static string Obfuscate(string s){
		char[] alpha = {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z','A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z','0','1','2','3','4','5','6','7','8','9','0','!','@','#','$','%','^','&','*','(',')','|',' '};
		return Obfuscate(alpha, s);
	}
	
	private static string DeObfuscate(char[] alpha, string s){
		if(s.Length + 2 > alpha.Length){
			return "!!!BAD DATA!!!";
		}
		System.Random grabRandomInt = new System.Random(); 
		StringBuilder sb = new StringBuilder(s);
		//grab the char keys pointing to where the original key is, and the char detailing how many added characters there are.
		//the second char key will always be the first in the string
		char deSecondCharKey = s[0];
		int deSecondIntKey = FindChar(alpha, deSecondCharKey);
		int deAddedLetters = FindChar(alpha, s[1]);
		//remove the first two keys from the string
		sb.Remove(0,2);
		char deFirstCharKey = s[deSecondIntKey+1];
		int deFirstIntKey = FindChar(alpha, deFirstCharKey);
		int a = 0;
		sb.Remove((deSecondIntKey - deAddedLetters - 1), sb.Length + 1 - (deSecondIntKey - deAddedLetters));
		for(int i=0; i<sb.Length; i++){
			grabRandomInt = new System.Random(deFirstIntKey + i);
			a = FindChar(alpha, sb[i]) - grabRandomInt.Next(0,alpha.Length - 1);
			if(a < 0){
				a += alpha.Length;
			}
			sb[i] = alpha[a];
		}
		//return the new string
		return sb.ToString();
	}
	
	private static string DeObfuscate(string s){
		char[] alpha = {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z','A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z','0','1','2','3','4','5','6','7','8','9','0','!','@','#','$','%','^','&','*','(',')','|',' '};
		return DeObfuscate(alpha, s);
	}
	
	private static int FindChar(char[] array, char toFind){
		for(int i=0; i<array.Length; i++){
			if(array[i] == toFind){
				return i;
			}
		}
		return -1;
	}
	
}
public class GameClient {
	public string name;
	public bool isHost;
}