using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Extensions;


[RequireComponent(typeof(DOPopup))]
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

	public void TakeCoin() {
		float value = slider.value;	
		Connect.shared.Emit(new Dictionary<string, object>(){
			{"f", "takecoin"},
			{"args", value}
		}, (json) => {
			var err = json.Int("err");
			if (err != 0) {
				// @TODO: TakeCoin失败
				Debug.Log("错误");
			} else {
				GConf.TakeCoinSuccess = true;
				gameObject.GetComponent<DOPopup>().Close();
			}
		});
	}

	void OnDestroy()
	{
		if (GConf.TakeCoinSuccess) {
			return ;
		}

		Connect.shared.Emit(new Dictionary<string, object>() {
			{"f", "unseat"}
		});
	}
}
