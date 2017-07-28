using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;
using System.Collections;
using System.Collections.Generic;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Awake () {
		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSocket("https://socket.dev.poker.top");
			external.SetProxy("http://localhost:8888");

            var rid = "597b102534b2f6375bda505f";
            var sid = "s%3A04zZLHTr4nPIma7doYIWAyKgehqvH4fY.2ijVVrPf%2BcxByqLsT1QViJaB2ybWCom6b9bhi6NsjVY";

			// 外网登录态
			// var sid = "s%3AHlY6SR0V3m8oM2oofbX_yl5R7f6v6Q7R.PK%2FqqIiSZHB0zLgH%2BwV52Yesi3CcsTPJFC3JPb7tjSQ";

            //external.InitGame(rid + "&" + sid);
			 external.InitMatch(rid + "&" + sid);
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

			var roomid = e.Data.Dict("myself").String("roomid");

			if (!string.IsNullOrEmpty(roomid)) {
				Connect.Shared.Emit(new Dictionary<string, object>{
					{"f", "entergame"},
					{"args", new Dictionary<string, object> {
						{"roomid", roomid},
						{"ver", Application.version},
						{"matchid", GameData.Shared.MatchID}
					}}
				});

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
