using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
public class Obfuscator{
	private static char[] chars = {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
								   'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
								   '0','1','2','3','4','5','6','7','8','9','0','!','@','#','$','%','^','&','*','(',')','|',' '};
	private static string Obfuscate(char[] alpha, string s){
		return s;
	}
	
	public static string Obfuscate(string s){
		return Obfuscate(chars, s);
	}
	
	private static string DeObfuscate(char[] alpha, string s){
		return s;
	}
	
	public static string DeObfuscate(string s){
		return DeObfuscate(chars, s);
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
