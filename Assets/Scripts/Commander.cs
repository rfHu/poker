using UnityEngine;
using System;

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

	public double[] Location() {
		char[] delimiter = {'&'};
		string[] result = ic.Location().Split(delimiter);

		if (result.Length < 2) {
			return new double[]{0 ,0};
		}

		return new double[]{
			Convert.ToDouble(result[0]),
			Convert.ToDouble(result[1])
		};
	}

	public void PayFor() {
		ic.PayFor();
	}
}

public interface ICommander {
	void Exit();
	string Location();
	void PayFor();
}

public class AndroidCommander: ICommander {
	private AndroidJavaObject getJo()  {
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		return jo;
	}

	public void Exit() {
		getJo().Call("closeGame");
	}

	public string Location() {
		return getJo().Call<string>("getLatitudeAndAltitude");
	} 

	public void PayFor(){
		getJo().Call("payFor");
	}
}

public class iOSCommander: ICommander {
	public void Exit(){}

	public string Location(){
		return "";
	}

	public void PayFor(){

	}
}
