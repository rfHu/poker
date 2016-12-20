using UnityEngine;
using System;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour {
	void Start () {
		DateTime time = System.DateTime.Now;	
		String timeStr = time.ToString("hh:mm");	
		
		GetComponent<Text>().text = timeStr;
	}
}
