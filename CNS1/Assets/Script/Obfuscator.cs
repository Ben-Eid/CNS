using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
public class Obfuscator{
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
	
	public static string Obfuscate(string s){
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
		if(deSecondIntKey < 0 || deAddedLetters < 0 || deFirstIntKey < 0){
			return "!!!BAD DATA!!!";
		}
		int a = 0;
		sb.Remove((deSecondIntKey - deAddedLetters - 1), sb.Length + 1 - (deSecondIntKey - deAddedLetters));
		for(int i=0; i<sb.Length; i++){
			grabRandomInt = new System.Random(deFirstIntKey + i);
			a = FindChar(alpha, sb[i]);
			if(a == -1){
				return "!!!BAD DATA!!!";
			}
			a -= grabRandomInt.Next(0,alpha.Length - 1);
			if(a < 0){
				a += alpha.Length;
			}
			sb[i] = alpha[a];
		}
		//return the new string
		return sb.ToString();
	}
	
	public static string DeObfuscate(string s){
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
