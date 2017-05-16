﻿using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3Awo5ufTp8DAcxxnkzMaiM401PsdgQNBNv.5rdImjJHRoEQvf8yX6M%2BHV2TJxtW5rgFIZucx2NnvVQ");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("591ad1eed174e864a1f59020");

		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
