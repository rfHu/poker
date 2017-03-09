﻿using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3AEXAas-DMsMiitXEyvN1dWb3saPZaL-ZU.jvmVGEHd3qX785w5%2BBcpRJSMKVbrtrGn5RKtUoCKVNE");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58c0f98b5eb5186cd67d7d3a"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
