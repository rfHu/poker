using UnityEngine;
using UnityEngine.UI;
using System;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Awake () {
		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSocket("https://socket.dev.poker.top");
			external.SetProxy("http://localhost:8888");
			// external.SetSid("s%3ASuOxhyXR0wmfu1xIZZUmFFdAq8Cd5xsD.h6RldH4l7niXxoZG0QyYnlt36QayIW0rtY0Z8GmorV0");
            // external.SetRoomID("5950a958cfb1ed3a3860c871");
                        external.SetRoomID("596768f43d52ea5f9b4d9744");

            external.SetSid("s%3AZrouZrFvN6GjVZVl-abHqdTvphzcTOUm.VY%2Bp4v4qoS7hr79tzXdBsaJ0xdnFPsV6k37WF5428%2F4");

            // external.SetSid("s%3A5oV4waP2FdytuZ_daHx1cOgW_0JwkJJ3.x7y5%2B%2BoEaDDtAZetL8CAiJVX2ZMOrUds%2BNP5IzyNjyc");
            // external.SetRoomID("595f73098425856d1c241a49");

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
