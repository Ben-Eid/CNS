using System.Collections;
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
				writer.WriteLine(Obfuscate(data));
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
		string[] aData = DeObfuscate(data).Split('|');
		Debug.Log(c.clientName + " Says: " + DeObfuscate(data));
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
	
	private string Obfuscate(char[] alpha, string s){
		if(s.Length + 2 > alpha.Length){
			return "!!!BAD DATA!!!";
		}
		StringBuilder sb = new StringBuilder();
		//choose a random letter, this will be the seed.
		int obFirstIntKey = UnityEngine.Random.Range(0,alpha.Length - 1);
		char obFirstCharKey = alpha[obFirstIntKey];
		int a = 0;
		for(int i=0; i<s.Length; i++){
			UnityEngine.Random.InitState(obFirstIntKey + i);
			a = UnityEngine.Random.Range(0,alpha.Length - 1);
			sb.Append(alpha[(FindChar(alpha, s[i]) + a) % alpha.Length]);
		}
		//choose another random letter, this will be put at the start of the string to point to where the seed key will be
		int obSecondIntKey = UnityEngine.Random.Range(s.Length + 1,alpha.Length);
		char obSecondCharKey = alpha[obSecondIntKey];
		//place the second key at the beginning of the string
		sb.Insert(0,obSecondCharKey);
		//add letters to make sure obFirstCharKey is in the right position and count how many new letters needed to be added
		int obAddedLetters = 0; 
		while(sb.Length < obSecondIntKey){
			obAddedLetters++;
			sb.Append(alpha[UnityEngine.Random.Range(0,alpha.Length - 1)]);
		}
		//place the character representation of the amount of added characters as the second char in the string.
		sb.Insert(1,alpha[obAddedLetters]);
		//places the firstcharkey at the position determined by the secondCharKey.
		sb.Append(obFirstCharKey);
		//add a bunch of random characters to the back. because these won't be used at all we do not need to keep track of them.
		int obEndCharacters = UnityEngine.Random.Range(0,20);
		for(int i=0; i<obEndCharacters && sb.Length + 2 < alpha.Length; i++){
			sb.Append(alpha[UnityEngine.Random.Range(0,alpha.Length - 1)]);
		}
		
		//return the new string
		Debug.Log("Obfuscation produces: " + sb.ToString());
		return sb.ToString();
	}
	
	private string Obfuscate(string s){
		char[] alpha = {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z','0','1','2','3','4','5','6','7','8','9','0','!','@','#','$','%','^','&','*','(',')','|',' '};
		return Obfuscate(alpha, s);
	}
	
	private string DeObfuscate(char[] alpha, string s){
		if(s.Length + 2 > alpha.Length){
			return "!!!BAD DATA!!!";
		}
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
			UnityEngine.Random.InitState(deFirstIntKey + i);
			a = FindChar(alpha, sb[i]) - UnityEngine.Random.Range(0,alpha.Length - 1);
			if(a < 0){
				a += alpha.Length;
			}
			sb[i] = alpha[a];
		}
		//return the new string
		return sb.ToString();
	}
	
	private string DeObfuscate(string s){
		char[] alpha = {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z','0','1','2','3','4','5','6','7','8','9','0','!','@','#','$','%','^','&','*','(',')','|',' '};
		return DeObfuscate(alpha, s);
	}
	
	private int FindChar(char[] array, char toFind){
		for(int i=0; i<array.Length; i++){
			if(array[i] == toFind){
				return i;
			}
		}
		return -1;
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