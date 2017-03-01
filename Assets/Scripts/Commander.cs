using UnityEngine;

public class Commander{
	private Commander(){}

	public static Commander Shared;

	public void Exit() {
		#if UNITY_ANDROID
			AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			jo.Call("closeGame");
		#endif

		Time.timeScale = 0;
	}
}
