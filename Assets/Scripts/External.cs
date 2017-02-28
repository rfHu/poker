﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class External : MonoBehaviour{
	public Text SidText;
	public Text RoomIDText;

	void Awake() {
		this.name = "External";
		Object.DontDestroyOnLoad(gameObject);
	}

	public void Exit() {
		Connect.Shared.Close();
		GameData.Shared.Sid = "";
		GameData.Shared.Room = "";

		// 返回上级界面
		SceneManager.LoadScene("GameLoading");
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

	public void SetProxy(string proxy) {
		GameData.Shared.Proxy = proxy;		
	}

	private void checkSetup() {
		if (string.IsNullOrEmpty(GameData.Shared.Sid) || string.IsNullOrEmpty(GameData.Shared.Room)) {
			return ;
		}

		Connect.Setup();	
	}
}
