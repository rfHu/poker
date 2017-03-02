using UnityEngine;
using System;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour {
	public Text timeText;
	public Text power;
	public Image rect;

	void Start () {
		
	}

	void Update()
	{
		DateTime time = System.DateTime.Now;	
		String timeStr = time.ToString("hh:mm");	
		timeText.text = timeStr;

		int battery = GetBatteryLevel();		
		power.text = battery.ToString() + "%";

		Vector2 size = rect.GetComponent<RectTransform>().sizeDelta;
		rect.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x * battery / 100, size.y);
	}

	public static int GetBatteryLevel()
    {
        return Commander.Shared.Power();
    }
}
