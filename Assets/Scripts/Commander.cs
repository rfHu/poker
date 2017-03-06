﻿using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

// Native提供接口给Unity
public class Commander {
	private Commander(){}

	public static Commander Shared = new Commander();

	private ICommander ic {
		get {
			#if UNITY_IOS
				return new iOSCommander();
			#elif UNITY_ANDROID
				return new AndroidCommander();
			#else
				return null;
			#endif
		}
	}	

	public void Exit() {
		Time.timeScale = 0;
		ic.Exit();
	}

	public IEnumerator Location(Action<float[]> success, Action fail) {
		if (!Input.location.isEnabledByUser) {
			fail();
			yield break;
		} 

		Input.location.Start();

		float maxWait = 3;

		while(Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
			yield return new WaitForFixedUpdate();
			maxWait = maxWait - Time.deltaTime;
		}

		// timeout或者获取位置失败
		if (maxWait <= 0 || Input.location.status == LocationServiceStatus.Failed) {
			fail();
			yield break;
		}

		success(new float[]{
			Input.location.lastData.longitude,
			Input.location.lastData.latitude
		});

		Input.location.Stop();
	}

	public void PayFor() {
		ic.PayFor();
	}

	public void GameEnd() {
		ic.GameEnd();
	}

	public int Power() {
		return ic.Power();
	}

	public void Audit() {
		ic.Audit();
	}
}

public interface ICommander {
	void Exit();
	void PayFor();
	void GameEnd();
	int Power();
	void Audit();
}

#if UNITY_ANDROID
public class AndroidCommander: ICommander {
	private AndroidJavaObject getJo()  {
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		return jo;
	}

	public void Exit() {
		getJo().Call("closeGame");
	}
	
	public void PayFor(){
		getJo().Call("payFor");
	}

	public void GameEnd() {
		getJo().Call("gameEnd", GameData.Shared.Room);
	}

	public int Power() {
		// 判断Unity_EDITOR方便调试
		#if UNITY_EDITOR
			return 50;	
		#else
			return getJo().Call<int>("getCurPower");
		#endif
	}

	public void Audit() {
		getJo().Call("jumpToVerify");
	}
}
#endif

#if UNITY_IOS
public class iOSCommander: ICommander {
	[DllImport("__Internal")]
	private static extern void _ex_callExitGame();

	[DllImport("__Internal")]
	private static extern void _ex_callOpenCoinMall();

	[DllImport("__Internal")]
	private static extern void _ex_callGameOver(String roomID);

	[DllImport("__Internal")]
	private static extern int _ex_callGetBatteryLevel();

	[DllImport("__Internal")]
	private static extern void _ex_callOpenAuditPage();

	public void Exit() {
		_ex_callExitGame();
	}
	
	public void PayFor() {
		_ex_callOpenCoinMall();
	}

	public void GameEnd() {
		_ex_callGameOver(GameData.Shared.Room);
	}

	public int Power() {
		return _ex_callGetBatteryLevel();
	}

	public void Audit() {
		_ex_callOpenAuditPage();
	}
}
#endif
