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

		// Debug.Log("Unity3D: Position=" + result);

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

	public void GameEnd() {
		ic.GameEnd();
	}

	public int Power() {
		return ic.Power();
	}

	public void Audit() {
		Audit();
	}
}

public interface ICommander {
	void Exit();
	string Location();
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

	public string Location() {
		return getJo().Call<string>("getLatitudeAndAltitude");
	} 

	public void PayFor(){
		getJo().Call("payFor");
	}

	public void GameEnd() {
		getJo().Call("gameEnd", GameData.Shared.Room);
	}

	public int Power() {
		return getJo().Call<int>("getCurPower");
	}

	public void Audit() {
		
	}
}
#endif

#if UNITY_IOS
public class iOSCommander: ICommander {
	[DllImport("__Internal")]
	private static extern void Exit(){}

	[DllImport("__Internal")]
	private static extern string Location(){
		return "";
	}

	[DllImport("__Internal")]
	private static extern void PayFor(){

	}

	[DllImport("__Internal")]
	private static extern void GameEnd() {

	}

	[DllImport("__Internal")]
	private static extern int Power() {
		return 1;
	}

	[DllImport("__Internal")]
	private static extern void Audit() {

	}

	public void Exit() {
		iOSCommander.Exit();
	}

	public string Location() {
		return iOSCommander.Location();
	}

	public void PayFor() {
		iOSCommander.PayFor();
	}

	public void GameEnd() {
		iOSCommander.GameEnd();
	}

	public int Power() {
		return iOSCommander.Power();
	}

	public void Audit() {
		iOSCommander.Audit();
	}
}
#endif
