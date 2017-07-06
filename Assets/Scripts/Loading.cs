using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour {
	public Text LoadingText;
	public string Txt;

	void Awake()
	{
		LoadingText.text = Txt;	
		// Debug.Log(transform.position);
	}
}
