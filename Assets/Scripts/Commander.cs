using UnityEngine;

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
		Time.timeScale = 0;
	}

	public string Location() {
		return "";
	}
}

public interface ICommander {
	void Exit();
	string Location();
}

public class AndroidCommander: ICommander {
	public void Exit() {
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		jo.Call("closeGame");
	}

	public string Location() {
		return "";
	} 
}

public class iOSCommander: ICommander {
	public void Exit(){}

	public string Location(){
		return "";
	}
}
