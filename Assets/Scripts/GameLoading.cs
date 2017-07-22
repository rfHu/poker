using UnityEngine;
using UnityEngine.UI;
using System;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Awake () {
		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSocket("https://socket.poker.top");
			external.SetProxy("http://localhost:8888");

			var rid = "597234f39363e91a44d29a79";
			var sid = "s%3AHlY6SR0V3m8oM2oofbX_yl5R7f6v6Q7R.PK%2FqqIiSZHB0zLgH%2BwV52Yesi3CcsTPJFC3JPb7tjSQ";

            external.InitGame(rid + "&" + sid);
			// external.InitMatch(rid + "&" + sid);
		#endif

		// 开启SDK的日志打印，发布版本请务必关闭
		//BuglyAgent.ConfigDebugMode (true);

		BuglyAgent.RegisterLogCallback (CallbackDelegate.Instance.OnApplicationLogCallbackHandler);

#if UNITY_IPHONE || UNITY_IOS
            BuglyAgent.InitWithAppId ("b3d868488f");
#endif

		// 如果你确认已在对应的iOS工程或Android工程中初始化SDK，那么在脚本中只需启动C#异常捕获上报功能即可
		BuglyAgent.EnableExceptionHandler ();
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
