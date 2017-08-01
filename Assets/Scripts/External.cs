﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UniRx;
using MaterialUI;

// Unity提供方法给Native
public class External : MonoBehaviour{
	private static External instance;
	private static bool statePaused = false;
	
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

	void Start() {
		this.name = "External";
		Commander.Shared.PauseUnity();
	}

	// 内部的退出接口都直接调用，外部只有当登陆互斥时才调用
	public void Exit() {
		if (Connect.Shared == null || Connect.Shared.IsClosed()) {
			ExitCb();
		} else {
			Connect.Shared.Close(ExitCb);			
		}
	}

	public void SendChat(String jsonStr) {
		RxSubjects.SendChat.OnNext(jsonStr);
	}

	public void ShowAudio(String jsonStr) {
		RxSubjects.ShowAudio.OnNext(jsonStr);
	}

	public void HideAudio(String jsonStr) {
		RxSubjects.HideAudio.OnNext(jsonStr);
	}

	public void ExitCb(Action callback) {
		close(callback);	
	}

	public void ExitCb() {
		close(() => {
			_.Log("UnityDebug: exit");
			Commander.Shared.Exit();
		});
	}

	public void Back2LoadingScene() {
		PoolMan.DespawnAll();
		SceneManager.LoadScene("GameLoading");
	}

	public void InitGame(string gameInfo) {
		var info = gameInfo.Split("&".ToCharArray());
		if (gameInfo.Length < 2) {
			return ;
		}

		GameData.Shared.Room = info[0].ToString();
		GameData.Shared.Sid = info[1].ToString();

		Connect.SetupRoom();
	}

	public void InitMatch(string gameInfo) {
		var info = gameInfo.Split("&".ToCharArray());
		if (gameInfo.Length < 2) {
			return ;
		}

		var matchID = info[0].ToString();
		var actScene = SceneManager.GetActiveScene().name;

		// 在PokerGame场景中切换比赛，跳回到GameLoading界面
		if (GameData.Shared.MatchID != matchID && actScene != "GameLoading") {
			SceneManager.LoadScene("GameLoading");
		}

		GameData.Shared.MatchID = matchID;
		GameData.Shared.Sid = info[1].ToString();

		Connect.SetupMatch();
	}

	public void SetProxy(string proxy) {
		Connect.Proxy = proxy;		
	}

	public void SetSocket(string domain) {
		Connect.Domain = domain;	
	}

	public void SetAPIDomain(string domain) {
		HTTP.APIDomain = domain;
	}

    public void CanPlay(string uid) {
        Commander.Shared.CanAudioPlay(GameData.Shared.FindPlayerIndex(uid) != -1);
    }

	private void close(Action callback) {
		// 清空关键数据
		GameData.Shared.Sid = "";
		GameData.Shared.Room = "";
		GameData.Shared.MatchID = "";

		Back2LoadingScene();

		// 延时执行退出逻辑
		Observable.Timer(TimeSpan.FromMilliseconds(90)).AsObservable().Subscribe((_) => {
			callback();
		});
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
			statePaused = true;

			_.Log("Unity: 游戏暂停");
			if (Connect.Shared == null) {
				return ;
			}
			// 暂停的时候，断开连接 
			Connect.Shared.CloseImmediate();
		} else {
			_.Log("Unity: 游戏恢复");
			var lastPaused = statePaused;
			statePaused = false;

			if (!lastPaused) {
				return ;	
			}

			if (!string.IsNullOrEmpty(GameData.Shared.MatchID)) {
				Connect.SetupMatch();
			} else {
				Connect.SetupRoom();
			}
		}
	}

	void OnApplicationFocus(bool focusStatus)
	{	
		// if (focusStatus) {
		// 	_.Log("focus");
		// } else {
		// 	_.Log("unfocus");
		// }
	}
}
