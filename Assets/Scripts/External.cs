﻿using UnityEngine;
using UnityEngine.UI;

public class External : MonoBehaviour{
	public Text SidText;
	public Text RoomIDText;

	void Awake() {
		this.name = "External";
		Object.DontDestroyOnLoad(gameObject);
	}

	public void Exit() {
		Connect.Shared.Close(() => {
			Application.Quit();		
		});
	}

	public void SetSid(string sid) {
		GameData.Shared.Sid = sid;
		SidText.text = "Sid: " + sid;
		checkSetup();		
	}

	public void SetRoomID(string roomID) {
		GameData.Shared.Room = roomID;
		RoomIDText.text = "RoomID: " + roomID;
		checkSetup();
	}

	private void checkSetup() {
		if (string.IsNullOrEmpty(GameData.Shared.Sid) || string.IsNullOrEmpty(GameData.Shared.Room)) {
			return ;
		}

		Connect.Shared.Setup();	
	}
}
