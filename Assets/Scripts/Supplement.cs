﻿using UnityEngine;
using UnityEngine.UI;
using UIWidgets;
using UnityEngine.Events;

public class Supplement : MonoBehaviour {
	public Text Blind;
	public Text Score;
	public Text Coins;
	public Text Pay;
	public Slider slider;

	// Use this for initialization
	void Start() {
		int score = GConf.bb * 100; 
		int min = GConf.bankroll[0] * score;
		int max = GConf.bankroll[1] * score;

		Blind.text = string.Format("{0}/{1}", GConf.bb / 2, GConf.bb);
		Coins.text = GConf.coins.ToString();
		OnChange(min);

		slider.minValue = min;
		slider.maxValue = max;
		slider.onValueChanged.AddListener(OnChange);

		// slider.ValueMin = GConf.bankroll[0] * score;
		// slider.ValueMax = (GConf.bankroll[1] - GConf.bankroll[0]) * score;
		// slider.LimitMin = slider.ValueMin;
		// slider.LimitMax = slider.ValueMax;
		// slider.Step = score;
		// slider.Value = score;
		// slider.OnValuesChange.AddListener(OnChange);
	}

	public void OnChange(float value) {
		int score = GConf.bb * 100; 

		int temp1 = Mathf.FloorToInt(value / score) * score;
		int temp2 = Mathf.CeilToInt(value / score) * score;
		int newValue;

		if (value - temp1 >= temp2 - value) {
			newValue = temp2;
 		} else {
			newValue = temp1;
		}

		slider.value = newValue;

		Score.text = newValue.ToString();
		Pay.text = (newValue * GConf.rake).ToString();
	}
}