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

		#if UNITY_EDITOR  
			external.SetSocket("https://socket.dev.poker.top");
			external.SetProxy("http://localhost:8888");

            // var rid = "599a959abbf3de4d88dcc599";
            var rid = "599cfb51270fef0ea73274f1";
            var sid = "s%3A88z03pRbCO1dRah4xGOvBqFaSUkwLl6e.c0TFcEOx5hssTvbShn3mWbwdQf7d8vMe%2F5eDGsrmj4c";

			// 外网登录态
			// var sid = "s%3AHlY6SR0V3m8oM2oofbX_yl5R7f6v6Q7R.PK%2FqqIiSZHB0zLgH%2BwV52Yesi3CcsTPJFC3JPb7tjSQ";

            external.InitGame(rid + "&" + sid);
			//external.InitMatch(rid + "&" + sid);
		#endif

		// 开启SDK的日志打印，发布版本请务必关闭
		//BuglyAgent.ConfigDebugMode (true);

		BuglyAgent.RegisterLogCallback (CallbackDelegate.Instance.OnApplicationLogCallbackHandler);

#if UNITY_IPHONE || UNITY_IOS
            BuglyAgent.InitWithAppId ("b3d868488f");
#endif

		// 如果你确认已在对应的iOS工程或Android工程中初始化SDK，那么在脚本中只需启动C#异常捕获上报功能即可
		BuglyAgent.EnableExceptionHandler ();

		registerEvents();
	}

	[SerializeField]private GameObject Loading;
	[SerializeField]private GameObject MTT; 

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
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
