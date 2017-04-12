using UnityEngine;
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
		ic.Exit();
	}

	public IEnumerator Location(Action<float[]> success, Action fail) {
		if (!Input.location.isEnabledByUser) {
			fail();
			yield break;
		} 

		Input.location.Start();

		float waitTime = 3;

		while(Input.location.status == LocationServiceStatus.Initializing && waitTime > 0) {
			yield return new WaitForFixedUpdate();
			waitTime = waitTime - Time.deltaTime;
		}

		// timeout或者获取位置失败
		if (waitTime <= 0 || Input.location.status == LocationServiceStatus.Failed) {
			fail();
		} else {
			success(new float[]{
				Input.location.lastData.longitude,
				Input.location.lastData.latitude
			});
		}

		Input.location.Stop();
	}

	public void PayFor() {
		ic.PayFor();
	}

	public void GameEnd(string roomID) {
		ic.GameEnd(roomID);
	}

	public int Power() {
		return ic.Power();
	}

	public void Audit() {
		ic.Audit();
	}

	public void Chat() {
		ic.Chat();
	}

    public void ShareRecord(int handID) {
        ic.ShareRecord(handID);
    }

    public void ShareGameRoom(string shareText) {
        ic.ShareGameRoom(shareText);
    }
}

public interface ICommander {
	void Exit();
	void PayFor();
	void GameEnd(string roomID);
	int Power();
	void Audit();
	void Chat();
    void ShareRecord(int handID);
    void ShareGameRoom(string shareText);
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

	public void GameEnd(string roomID) {
		getJo().Call("gameEnd", roomID);
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

	public void Chat() {
		getJo().Call("openChat");
	}

    public void ShareRecord(int handID) {
        getJo().Call("shareGameRecord", handID);
    }

    public void ShareGameRoom(string shareText) {
        getJo().Call("shareGame", shareText);
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
	private static extern void _ex_callGameOver(string roomID);

	[DllImport("__Internal")]
	private static extern int _ex_callGetBatteryLevel();

	[DllImport("__Internal")]
	private static extern void _ex_callOpenAuditPage();

	[DllImport("__Internal")]
	private static extern void _ex_callGameChatting();

	[DllImport("__Internal")]
	private static extern void _ex_callShareRoomRecord(int handID);

	[DllImport("__Internal")]
	private static extern void _ex_callShareRoom(string text);

	public void Exit() {
		_ex_callExitGame();
	}
	
	public void PayFor() {
		_ex_callOpenCoinMall();
	}

	public void GameEnd(string roomID) {
		_ex_callGameOver(roomID);
	}

	public int Power() {
#if UNITY_EDITOR
			return 50;
#else
			return _ex_callGetBatteryLevel();
#endif
	}

	public void Audit() {
		_ex_callOpenAuditPage();
	}

	public void Chat() {
		_ex_callGameChatting();
	}

    public void ShareRecord(int handID) {
        _ex_callShareRoomRecord(handID);
    }

    public void ShareGameRoom(string shareText) {
        _ex_callShareRoom(shareText);
    }
}
#endif
