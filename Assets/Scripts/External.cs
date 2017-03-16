﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System;

// Unity提供方法给Native
public class External : MonoBehaviour{
	private static External instance;
	
	public static External Instance {
		get {
			if (instance == null) {
				GameObject go = new GameObject();
				UnityEngine.Object.DontDestroyOnLoad(go);
				
				instance = go.AddComponent<External>();
			}

			return instance;
		}
	}

	void Awake() {
		this.name = "External";
	}

	// 内部的退出接口都直接调用，外部只有当登陆互斥时才调用
	public void Exit() {
		if (Connect.Shared == null || Connect.Shared.IsClosed()) {
			ExitCb();
		} else {
			Connect.Shared.Close(ExitCb);			
		}
	}

	public void ExitCb(Action callback) {
		close(callback);	
	}

	public void ExitCb() {
		close(() => {
			Debug.Log("UnityDebug: exit");
			Commander.Shared.Exit();
		});
	}
	
	public void SetSid(string sid) {
		Debug.Log("Unity: Sid=" + sid);
		GameData.Shared.Sid = sid;
		checkSetup();		
	}

	public void SetRoomID(string roomID) {
		Debug.Log("Unity: roomID=" + roomID);
		GameData.Shared.Room = roomID;
		checkSetup();
	}

	public void SetProxy(string proxy) {
		GameData.Shared.Proxy = proxy;		
	}

	private void checkSetup() {
		if (string.IsNullOrEmpty(GameData.Shared.Sid) || string.IsNullOrEmpty(GameData.Shared.Room)) {
			return ;
		}

		// 开始游戏
		Time.timeScale = 1;
		Connect.Setup();	
	}

	private void close(Action callback) {
		// 清空两个关键数据
		GameData.Shared.Sid = "";
		GameData.Shared.Room = "";

		// 返回上级界面
		#if UNITY_EDITOR
			// Nothing to do
		#else
			SceneManager.LoadScene("GameLoading");
			callback();
		#endif
	}

	void OnApplicationQuit()
	{
		if (Connect.Shared == null) {
			return ;
		}

		#if UNITY_EDITOR
			Connect.Shared.CloseImmediate();
		#else
			Connect.Shared.Close();
		#endif
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus) {
			if (Connect.Shared == null) {
				return ;
			}

			// 暂停的时候，断开连接 
			Connect.Shared.CloseImmediate();
		} else {
			Connect.Setup();
		}
	}

	void OnApplicationFocus(bool focusStatus)
	{	
		// if (focusStatus) {
		// 	Debug.Log("focus");
		// } else {
		// 	Debug.Log("unfocus");
		// }
	}
}
