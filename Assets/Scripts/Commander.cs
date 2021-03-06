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
		#if UNITY_EDITOR
		#else
			ic.Exit();
		#endif
	}

	public void PauseUnity() {
		ic.PauseUnity();
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

	public void GameEnd(string roomID, string gameType) {
		ic.GameEnd(roomID, gameType);
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

    public void VoiceIconToggle(bool isShowing) {
		#if UNITY_EDITOR
			return ;
		#endif
        ic.VoiceIconToggle(isShowing);
    }

    public void OptionToggle(bool isOpen, int type) {
        ic.OptionToggle(isOpen, type);
    }

    public void ShareSNGResult(string pic = null) {
        ic.ShareSNGResult(pic);
    }

    public void InitHx(string chatRoomId) {
		#if UNITY_EDITOR
			return ;
		#endif
        ic.InitHx(chatRoomId);
    }

    public void CanAudioPlay(bool canPlay) {
        ic.CanAudioPlay(canPlay);
    }

    public void CloseChat() {
        ic.CloseChat();
    }
}

public interface ICommander {
	void Exit();
	void PayFor();
	void GameEnd(string roomID, string gameType);
	int Power();
	void Audit();
	void Chat();
    void ShareRecord(int handID);
    void ShareGameRoom(string shareText);
    void ShareSNGResult(string pic);
    void VoiceIconToggle(bool isShowing);
	void PauseUnity();
    void OptionToggle(bool isOpen, int type);
    void InitHx(string chatRoomId);
    void CanAudioPlay(bool canPlay);
    void CloseChat();
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

	public void GameEnd(string roomID, string gameType) {
		getJo().Call("gameEnd", roomID, gameType);
	}

	public int Power() {
		// 判断Unity_EDITOR方便调试
		#if UNITY_EDITOR
			return 50;	
		#else
			try {
				return getJo().Call<int>("getCurPower");
			} catch(Exception e) {
				return 50;
			}
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

    public void VoiceIconToggle(bool isShowing) {
		try {
        	getJo().Call("voiceIconToggle", isShowing);
		} catch(Exception e) {
			// skip
		}
    }

    public void ShareSNGResult(string pic) {
        getJo().Call("shareSNGResult", pic);
    }

	public void PauseUnity(){}

    public void OptionToggle(bool isOpen, int type) 
    {
		#if UNITY_EDITOR 
		#else
			try {

			} catch(Exception e) {
	        	getJo().Call("gameMessageIsShowToggle", isOpen, type);
			}
		#endif
    }

    public void InitHx(string chatRoomId)
    {
        getJo().Call("initHx", chatRoomId);
    }

    public void CanAudioPlay(bool canPlay)
    {
        getJo().Call("canAudioPlay", canPlay);
    }

    public void CloseChat() 
    {
        getJo().Call("closeChat");
    }

	public string GetLocation() {
		return getJo().Call<string>("getPlayerLocation");
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
	private static extern void _ex_callGameOver(string roomID, string gameType);

	[DllImport("__Internal")]
	private static extern void _ex_callCanAudioPlay(bool canPlay);

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

	[DllImport("__Internal")]
	private static extern  void _ex_callVoiceIconState(bool isShow);

	[DllImport("__Internal")]
	private static extern void _ex_pauseUnity();

	[DllImport("__Internal")]
	private static extern void _ex_callOptionToggle(bool isOpen, int type);

	[DllImport("__Internal")]
	private static extern void _ex_callShareSNGResult();

    [DllImport("__Internal")]
    private static extern void _ex_callInitHx(string chatRoomId);

    [DllImport("__Internal")]
    private static extern void _ex_callCloseChatting();

	public void Exit() {
		_ex_callExitGame();
	}
	
	public void PayFor() {
		_ex_callOpenCoinMall();
	}

	public void GameEnd(string roomID, string gameType) {
		_ex_callGameOver(roomID, gameType);
	}

	public void PauseUnity() {
#if UNITY_EDITOR
#else
		_ex_pauseUnity();
#endif
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

    public void VoiceIconToggle(bool isShowing) {
        _ex_callVoiceIconState(isShowing);
    }

    public void ShareSNGResult(string pic) {
        _ex_callShareSNGResult();
    }

    public void OptionToggle(bool isOpen, int type) {
#if UNITY_EDITOR
#else
        	_ex_callOptionToggle(isOpen, type);
#endif
    }

    public void InitHx(string chatRoomId) {
        _ex_callInitHx(chatRoomId);
    }

	public void CanAudioPlay(bool canPlay) {
		_ex_callCanAudioPlay(canPlay);
	}

	public void CloseChat() {
		_ex_callCloseChatting();
	}
}
#endif
