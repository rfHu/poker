using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;
using System.Collections;
using System.Collections.Generic;

public class GameLoading : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		Application.targetFrameRate = 60;
		var external = External.Instance;

		// 事件监听
		registerEvents();

		debugSetup();

		BuglyAgent.RegisterLogCallback (CallbackDelegate.Instance.OnApplicationLogCallbackHandler);
		#if UNITY_IPHONE || UNITY_IOS
			BuglyAgent.InitWithAppId ("b3d868488f");
		#endif

		// 如果你确认已在对应的iOS工程或Android工程中初始化SDK，那么在脚本中只需启动C#异常捕获上报功能即可
		BuglyAgent.EnableExceptionHandler ();
	}

	[SerializeField]private GameObject Loading;
	[SerializeField]private GameObject MTT; 

	[SerializeField]private GameObject loadingScene;
	[SerializeField]private GameObject gameScene;

	public void Exit() {
		External.Instance.Exit();
	}

	private void registerEvents() {
		RxSubjects.MatchLook.Subscribe((e) => {
			var state = e.Data.Int("match_state");
			if (state >= 10) {
				PokerUI.ToastThenExit("未匹配到您的比赛信息");
				return ;
			}

			Loading.SetActive(false);
			MTT.SetActive(true);
			MTT.GetComponent<MTTWaiting>().Init(e.Data);
		}).AddTo(this);

		RxSubjects.Look.Subscribe((e) => {
			if (loadingScene.activeInHierarchy) {
				loadingScene.SetActive(false);
				gameScene.transform.parent.gameObject.SetActive(true);
				gameScene.SetActive(true);

				gameScene.GetComponent<Controller>().ReEnter();

				// 打开声音发送按钮
				Commander.Shared.VoiceIconToggle(true);
			} 	
		}).AddTo(this);

		RxSubjects.GameReset.Subscribe((_) => {
			back2LoadingScene();	
		}).AddTo(this);
	}

	private void resetLoading() {
		loadingScene.SetActive(true);
		Loading.SetActive(true);
		MTT.SetActive(false);
		Loading.GetComponent<Loading>().SetRndText();	
	}

	private void back2LoadingScene() {
		resetLoading();

		if(!gameScene.activeInHierarchy) {
			return ;
		}

		PoolMan.DespawnAll();

		gameScene.transform.parent.gameObject.SetActive(false);

		// 关闭声音发送按钮
		Commander.Shared.VoiceIconToggle(false);

		#if UNITY_EDITOR
			// // 切换房间测试逻辑
			// var rid = "59c38cbe6a007e545ed0a2ae";
			// var sid = "s%3ALq49kS4bIomBNiKEq8yHbXHFCavRAPXC.EJiBchVuMVcoXilDMTY6uy1cKgnM55rKbSp9zZrGfY0";
			// External.Instance.InitGame(rid + "&" + sid);

			debugSetup();
		#endif
	}

    private void debugSetup() {
        #if UNITY_EDITOR 
			External.Instance.SetSocket("https://socket.dev.poker.top");
			External.Instance.SetProxy("http://localhost:8888");

            var rid = "59cef928ed94f550e84c8a4c";
            var sid = "s%3AtGAkUO0BXaoCSZR7o43Ct0PklBZHEVYd.hadhC%2Bex6IkLykaLPe4qNd8RXl0wa%2FXEinv0qBR2BdM";

            //External.Instance.InitGame(rid + "&" + sid); 
        External.Instance.InitGame(rid + "&" + sid);
		#endif
    }
}
