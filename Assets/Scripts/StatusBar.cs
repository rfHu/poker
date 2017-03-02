using UnityEngine;
using System;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour {
	public Text timeText;
	public Text power;
	public Image rect;

	private float width;
	private float height;

	void Start () {
		var size = rect.GetComponent<RectTransform>().sizeDelta;
		width = size.x;
		height = size.y;
	}

	void Update()
	{
		DateTime time = System.DateTime.Now;	
		String timeStr = time.ToString("hh:mm");	
		timeText.text = timeStr;

		int battery = GetBatteryLevel();		
		power.text = battery.ToString() + "%";

		rect.GetComponent<RectTransform>().sizeDelta = new Vector2(width * battery / 100, height);
	}

	public static int GetBatteryLevel()
    {
        return Commander.Shared.Power();
    }
}
