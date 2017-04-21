﻿using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        	external.SetSid("s%3AcWb4R4muxh-isFoOGqAQZS2_rf5UNMJt.nCR2EBzPu3h%2BrKxHoSRiJwpk%2FxJCKaq1xvaZXS4eNUI");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58f9a2f05630a67fe7d26b47");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
