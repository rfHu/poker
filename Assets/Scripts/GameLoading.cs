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
			if (loadingScene.activeSelf) {
				loadingScene.SetActive(false);
				gameScene.transform.parent.gameObject.SetActive(true);
				gameScene.SetActive(true);

				gameScene.GetComponent<Controller>().ReEnter();

				// 打开声音发送按钮
				Commander.Shared.VoiceIconToggle(true);
			} 	
		}).AddTo(this);

		RxSubjects.GameEnter.Subscribe((_) => {
			PoolMan.DespawnAll();

			loadingScene.SetActive(true);
			Loading.SetActive(true);
			MTT.SetActive(false);

			gameScene.transform.parent.gameObject.SetActive(false);

			// 关闭声音发送按钮
			Commander.Shared.VoiceIconToggle(false);

			#if UNITY_EDITOR
				// // 切换房间测试逻辑
				// var rid = "59c327043cd0b31b60c94849";
				// var sid = "s%3AZCRd1PKK5XocACGHDIhVIJ1XLw35WmZE.MBMn43t7zVkyfY3xWVJRC4f6Xl4ARR447Hk6SN8D93Q";
				// External.Instance.InitGame(rid + "&" + sid);

				debugSetup();
			#endif


			Loading.GetComponent<Loading>().SetRndText();
		}).AddTo(this);
	}

    private void debugSetup() {
        #if UNITY_EDITOR 
			External.Instance.SetSocket("https://socket.dev.poker.top");
			External.Instance.SetProxy("http://localhost:8888");
			// var rid = "59c1e1928874674cfac303b3";
			// var sid = "s%3AsAgz8nuZ15cD6lyIv6zC1hfUABIpTukA.cXisQhX16Lgqf1YB8ZjZzw4I1%2FIe40%2FHyQDQzRnPpXo";

			// External.Instance.InitGame(rid + "&" + sid);

            var rid = "59c332393cd0b31b60c95bdd";
            var sid = "s%3AqO3LuM1oxpS3q2tKLYWDofquy7TQ7Zy-.UIWsRAQCWfFEKqZasgNyOgXxNwNxCL%2BZbACFkobI3n4";

			External.Instance.InitGame(rid + "&" + sid);
		#endif
    }
}
