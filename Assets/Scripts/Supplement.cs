using UnityEngine;
using UnityEngine.UI;
using UIWidgets;
using UnityEngine.Events;

public class Supplement : MonoBehaviour {
	public Text Blind;
	public Text Score;
	public Text Coins;
	public Text Pay;
	public CenteredSlider slider;

	// Use this for initialization
	void Start() {
		int score = GConf.bb * 100; 

		Blind.text = string.Format("{0}/{1}", GConf.bb / 2, GConf.bb);
		Score.text = score.ToString();
		Coins.text = GConf.coins.ToString();
		Pay.text = ((float)score * GConf.rake).ToString();

		slider.ValueMin = GConf.bankroll[0] * score;
		slider.ValueMax = (GConf.bankroll[1] - GConf.bankroll[0]) * score;
		slider.Step = score;
		slider.Value = score;

		slider.OnValuesChange.AddListener(OnChange);
	}

	public void OnChange(int value) {
		Score.text = value.ToString();
		Pay.text = ((float)value * GConf.rake).ToString();
	}
}
