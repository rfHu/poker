﻿using UnityEngine;

public class GameLoading : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;
		external.SetSid("s%3AS0RVvKWrKE83MwBLgQKHn2DReSNMfVz-.PHE19IuTAKKj%2BaCGCgrz7dOsPAzjccGk7YmiuCAvF4s");
		external.SetProxy("http://localhost:8888");
		external.SetRoomID("58b63dcfe0e22a13382f018a"); // 无需审核
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
