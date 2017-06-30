using UnityEngine;
using UnityEngine.UI;
using System;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Awake () {
		var external = External.Instance;
		ObjectsPool.Setup();

		#if UNITY_EDITOR  
			external.SetSocket("https://socket.dev.poker.top");
			external.SetProxy("http://localhost:8888");
			// external.SetSid("s%3ASuOxhyXR0wmfu1xIZZUmFFdAq8Cd5xsD.h6RldH4l7niXxoZG0QyYnlt36QayIW0rtY0Z8GmorV0");
            // external.SetRoomID("5950a958cfb1ed3a3860c871");
            external.SetSid("s%3AO2DA-A5cIVyNCjJmmr1LFjdcyN8QIFDt.aA8WJf0aU8Ym%2BNKXUuN%2FggKfKQM928G0sAhqb1ylEWk");
            external.SetRoomID("595612388578b423c145ef14");

		#endif

		// 开启SDK的日志打印，发布版本请务必关闭
		// BuglyAgent.ConfigDebugMode (true);
		// 注册日志回调，替换使用 'Application.RegisterLogCallback(Application.LogCallback)'注册日志回调的方式
		// BuglyAgent.RegisterLogCallback (CallbackDelegate.Instance.OnApplicationLogCallbackHandler);
		#if UNITY_IPHONE || UNITY_IOS
			BuglyAgent.InitWithAppId ("b3d868488f");
		#elif UNITY_ANDROID
			BuglyAgent.InitWithAppId ("a8103a785a");
		#endif

		// 如果你确认已在对应的iOS工程或Android工程中初始化SDK，那么在脚本中只需启动C#异常捕获上报功能即可
		BuglyAgent.EnableExceptionHandler ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
