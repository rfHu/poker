using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Commander {
	private Commander(){}

	public static Commander Shared = new Commander();

	public void Exit() {
		#if UNITY_ANDROID
			AndroidJavaClass jc = new AndroidJavaClass("texas.poker.top.activity.GameActivity");
			AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			jo.Call("closeGame");
		#endif
	}
}
