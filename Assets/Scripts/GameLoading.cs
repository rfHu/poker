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

		LoadingGo = MTT.transform.parent.gameObject;

		// 事件监听
		registerEvents();

        var debug = false;
        #if UNITY_EDITOR 
        debug = true;
		#endif

        if (Debug.isDebugBuild || debug)
        {
            debugSetup();
        }

		BuglyAgent.RegisterLogCallback (CallbackDelegate.Instance.OnApplicationLogCallbackHandler);
		#if UNITY_IPHONE || UNITY_IOS
					BuglyAgent.InitWithAppId ("b3d868488f");
		#endif

		// 如果你确认已在对应的iOS工程或Android工程中初始化SDK，那么在脚本中只需启动C#异常捕获上报功能即可
		BuglyAgent.EnableExceptionHandler ();
	}

	[SerializeField]private GameObject Loading;
	[SerializeField]private GameObject MTT; 

	private GameObject LoadingGo;

	[SerializeField]private GameObject GameCanvas;	

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
			if (LoadingGo.activeSelf) {
				LoadingGo.SetActive(false);
				GameCanvas.SetActive(true);	
			} 
		}).AddTo(this);
	}

    private void debugSetup() {
        External.Instance.SetSocket("https://socket.poker.top");
        External.Instance.SetProxy("http://localhost:8888");
        var rid = "59b92dbbea016f2ba72bf289";
		var sid = "s%3At0oBMcZ2aTiXaHPb7K01z82IO2sDpDlJ.DWLduOoLfiGkTXUFUDY4xMsBtfaRhJxzswkLu9P5fy4";

        External.Instance.InitGame(rid + "&" + sid);
    }
}
