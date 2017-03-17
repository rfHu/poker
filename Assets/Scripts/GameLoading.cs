﻿using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3AdZWGawfPW_hfdXIWs1HgD4kAdjc5ILDd.PjrPJXv0npDJmvxQ%2Bq%2FreoattuteVrMG9iGM%2FO89Xn4");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58cb7ed9a796eb0ccdcf9896"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
