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

	private void registerEvents() {
		RxSubjects.Connecting.Subscribe((e) => {
			Loading.SetActive(true);
		}).AddTo(this);

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
	}

	public void ExitGame() {
		External.Instance.Exit();
	}

    private void debugSetup() {
        External.Instance.SetSocket("https://socket.dev.poker.top");
        External.Instance.SetProxy("http://localhost:8888");
        var rid = "59b00d5522a3c94f514b5ee4";
        // var sid = "s%3AdTHMLRWf9UngWkjvKvKyL53l6dmQwYWt.ECGuDvFU1zSId15SSJ3tRPLKutRU6UM75B%2F0LyeIjog";
        var sid = "s%3AJapnJs2n2FeWhjQReyFw72VhwoFSD1po.S6xGI00tV8K61zqUw0yha0ec547iypYT%2FfpcYSQSAsA";

        //External.Instance.InitMatch(rid + "&" + sid);
        External.Instance.InitGame(rid + "&" + sid);
    }
}
