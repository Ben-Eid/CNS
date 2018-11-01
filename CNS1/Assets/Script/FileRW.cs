using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class FileRW{
	public static string[] ReadFile(string path, char split){
		string[] data = new string[0];
		StreamReader reader = new StreamReader(path);
		data = (reader.ReadToEnd()).Split(split);
		reader.Close();
		return data;
	}
}
