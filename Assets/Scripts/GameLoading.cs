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
			if (loadingScene.activeSelf) {
				loadingScene.SetActive(false);
				gameScene.SetActive(true);

				// 打开声音发送按钮
				Commander.Shared.VoiceIconToggle(true);
			} 	
		}).AddTo(this);

		RxSubjects.GameExit.Subscribe((_) => {
			loadingScene.SetActive(true);
			gameScene.SetActive(false);

			// 关闭声音发送按钮
			Commander.Shared.VoiceIconToggle(false);

			// 编辑器模式下会自动重新加载
			debugSetup();

			Loading.GetComponent<Loading>().SetRndText();
		}).AddTo(this);
	}

    private void debugSetup() {
        #if UNITY_EDITOR 
			External.Instance.SetSocket("https://socket.dev.poker.top");
			External.Instance.SetProxy("http://localhost:8888");
            var rid = "59c0b60e95fbcb31eeb81dd9";
			var sid = "s%3AZ06oeIzrYYy-1uBWltK-jRmhAbY93Nx1.R3%2F2YtIs6Z4CZO8qah2ABz2zYB7i1rWBv7U%2BI7BHjqg";

			External.Instance.InitGame(rid + "&" + sid);
		#endif
    }
}
