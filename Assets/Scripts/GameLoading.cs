﻿using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  

        external.SetSid("s%3A4eSBkdCamoddL5vRwIBJIxc1eaOTquhg.%2FaSitkmQ%2B0a8rz02KM%2B3YaXaGUX6jneMEU6fgF31NF4");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58e45f5816b8c759eecba7a9");

		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
