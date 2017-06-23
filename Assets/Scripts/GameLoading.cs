﻿using UnityEngine;
using UnityEngine.UI;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Awake () {
		var external = External.Instance;
		ObjectsPool.Setup();

		#if UNITY_EDITOR  
        	// external.SetSid("s%3AdOlm2w5hGhqahgt--HMRhxyXjTCSmOLf.3IXxXmGw1R3E0Swr10JEcqYcGvn7AUZT4%2F0d99jOuT8");
			external.SetSocket("https://socket.poker.top");
        	external.SetSid("s%3A4gHJE4AJbNY5j6XT2OuoeO4-nqn6bl7t.5f5shYMZ2sFNrz0CKTpO5b2cUJujPknpA5OU7lThbJI");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("594d0f3e532d9919716bb881");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
