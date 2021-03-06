﻿using UnityEngine;
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
			var rid = "59e744a6c3342612ea8fb53d";
			var sid = "s%3AsF7HMvYzII20fOFynbzgvaJd8qAo09kk.IT7xTx2KDUjs2KPHZA6qmuj1FCC5ySCOR%2F8gYs69J7Q";
			External.Instance.InitGame(rid + "&" + sid);

			// debugSetup();
		#endif
	}

    private void debugSetup() {
        #if UNITY_EDITOR 
			External.Instance.SetSocket("https://socket.poker.top");
			// External.Instance.SetProxy("http://localhost:8888");

            var rid = "5a7d549c9134dd5e8a1bfb0d";
            var sid = "s%3AyBDfpmLQfM1fWxjWruDB36SAagY4E7jE.Ftdh9Yo%2BbsXjJcewBDdYQ7WkAkwsAjkb3YJPByO9TOg";
        // var cid = "59dc88962e3bc405807781f6";
        //External.Instance.InitGame(rid + "&" + sid); 
        External.Instance.InitGame(rid + "&" + sid);
		#endif
    }
}
