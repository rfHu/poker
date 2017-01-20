using UnityEngine;
using System;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour {
	public Text timeText;
	public Text power;
	public Image rect;

	void Start () {
		DateTime time = System.DateTime.Now;	
		String timeStr = time.ToString("hh:mm");	
		timeText.text = timeStr;

		int battery = (int) GetBatteryLevel();		
		power.text = battery.ToString() + "%";

		Vector2 size = rect.GetComponent<RectTransform>().sizeDelta;
		rect.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x * battery / 100, size.y);
	}

	// void OnGUI()
	// {
	// 	Color color = new Color(255, 255, 255, 255);
	// 	Texture2D texture = new Texture2D(1, 1);
	// 	texture.SetPixels(new Color[]{color});
	// 	texture.Apply();

	// 	GUIStyle style = new GUIStyle(GUI.skin.box);
	// 	style.normal.background = texture;
	// 	GUI.Box(new Rect(0, 0, 50, 30), "", style);
	// }

	public static float GetBatteryLevel()
    {
        return 100;
    }
}
