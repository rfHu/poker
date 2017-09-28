using UnityEngine;
using System;
using UniRx;
using MaterialUI;

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

	private bool needReset = false;

	public void InitGame(string gameInfo) {
		var info = gameInfo.Split("&".ToCharArray());
		if (gameInfo.Length < 2) {
			return ;
		}

		if (needReset) {
			RxSubjects.GameReset.OnNext(true);
		}
		needReset = true;

		// RxSubjects.Connecting.OnNext(true);

		GameData.Shared.Room.Value = info[0].ToString();
		GameData.Shared.Sid = info[1].ToString();

		GameData.Shared.IsMatchState = false;
		Connect.Setup();
	}

	public void InitMatch(string gameInfo) {
		var info = gameInfo.Split("&".ToCharArray());
		if (gameInfo.Length < 2) {
			return ;
		}

		if (needReset) {
			RxSubjects.GameReset.OnNext(true);
		}
		needReset = true;

		var matchID = info[0].ToString();

		GameData.Shared.MatchID = matchID;
		GameData.Shared.Sid = info[1].ToString();

		GameData.Shared.IsMatchState = true;
		Connect.Setup();
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
        Commander.Shared.CanAudioPlay(GameData.Shared.FindPlayerIndex(uid) != -1 && !GameData.Shared.InsuranceState.Value);
    }

	private void close(Action callback) {
		// 清空关键数据
		GameData.Shared.Sid = "";
		GameData.Shared.Room.Value = "";
		GameData.Shared.MatchID = "";

		// 延时执行退出逻辑
		Observable.Timer(TimeSpan.FromMilliseconds(90)).AsObservable().Subscribe((_) => {
			RxSubjects.GameReset.OnNext(true);
			needReset = false;
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
		#if UNITY_EDITOR
			return ;
		#endif

		if (pauseStatus) {
			_.Log("Unity: 游戏暂停");
			if (Connect.Shared == null) {
				return ;
			}
			// 暂停的时候，断开连接 
			Connect.Shared.CloseImmediate();
		} else {
			_.Log("Unity: 游戏恢复");
			Connect.Setup();	
		}
	}

	void OnApplicationFocus(bool focusStatus)
	{	
		
	}
}
