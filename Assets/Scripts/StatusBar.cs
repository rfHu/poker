﻿using UnityEngine;
using System;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour {
	public Text timeText;
	public Text power;

	void Start () {
		DateTime time = System.DateTime.Now;	
		String timeStr = time.ToString("hh:mm");	
		timeText.text = timeStr;
		
		power.text = ((int) GetBatteryLevel()).ToString() + "%";
	}

	void OnGUI()
	{
		Color color = new Color(255, 255, 255, 255);
		Texture2D texture = new Texture2D(1, 1);
		texture.SetPixels(new Color[]{color});
		texture.Apply();

		GUIStyle style = new GUIStyle(GUI.skin.box);
		style.normal.background = texture;
		GUI.Box(new Rect(0, 0, 100, 100), "", style);
	}

	public static float GetBatteryLevel()
    {
        #if UNITY_IOS
        UIDevice device = UIDevice.CurrentDevice();
        device.batteryMonitoringEnabled = true; // need to enable this first
        Debug.Log("Battery state: " + device.batteryState);
        Debug.Log("Battery level: " + device.batteryLevel);
        return device.batteryLevel*100;
        #elif UNITY_ANDROID
 
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    if (null != unityPlayer)
                    {
                        using (AndroidJavaObject currActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                        {
                            if (null != currActivity)
                            {
                                using (AndroidJavaObject intentFilter = new AndroidJavaObject("android.content.IntentFilter", new object[]{ "android.intent.action.BATTERY_CHANGED" }))
                                {
                                    using (AndroidJavaObject batteryIntent = currActivity.Call<AndroidJavaObject>("registerReceiver", new object[]{null,intentFilter}))
                                    {
                                        int level = batteryIntent.Call<int>("getIntExtra", new object[]{"level",-1});
                                        int scale = batteryIntent.Call<int>("getIntExtra", new object[]{"scale",-1});
 
                                        // Error checking that probably isn't needed but I added just in case.
                                        if (level == -1 || scale == -1)
                                        {
                                            return 50f;
                                        }
                                        return ((float)level / (float)scale) * 100.0f;
                                    }
                               
                                }
                            }
                        }
                    }
                }
            } catch (System.Exception)
            {
             
            }
        }
     
        return 100;
        #endif
    }
}
