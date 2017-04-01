﻿using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  

        external.SetSid("s%3AuuLS14oIFCHuupeFXUTpriyvDfWPVX7v.UT4sMBS%2FIwGCHAoCuJyR1VcsZuTn1%2FVI%2FDEoki%2FDrQw");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58de1ca05d5a4b2fcae3c85f");

		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
