using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Extensions;
using UniRx;
using UIWidgets;

[RequireComponent(typeof(DOPopup))]
public class Supplement : MonoBehaviour {

	public Text Blind;
	public Text Score;
	public Text Coins;
	public Text Pay;
	public Slider slider;

	// Use this for initialization
	void Awake() {
		int score = GameData.Shared.BB * 100; 
		int min = GameData.Shared.BankrollMul[0] * score;
		int max = GameData.Shared.BankrollMul[1] * score;

		Blind.text = string.Format("{0}/{1}", GameData.Shared.BB / 2, GameData.Shared.BB);
		Coins.text = GameData.Shared.Coins.ToString();
		OnChange(min);

		slider.minValue = min;
		slider.maxValue = max;
		slider.onValueChanged.AddListener(OnChange);

		RxSubjects.UnSeat.AsObservable().Where((e) => {
			var uid = e.Data.String("uid");
			return GameData.Shared.Uid == uid;
		}).Subscribe((e) => {
			GetComponent<DOPopup>().Close();
		}).AddTo(this);
	}

	public void OnChange(float value) {
		int score = GameData.Shared.BB * 100; 

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
		Pay.text = (newValue * GameData.Shared.Rake).ToString();
	}

	public void TakeCoin() {
		float value = slider.value;	
		Connect.Shared.Emit(new Dictionary<string, object>(){
			{"f", "takecoin"},
			{"args", value}
		}, (json) => {
			var err = json.Int("err");
			if (err != 0) {
				PokerUI.ShowDialog("金币不足，请购买", new DialogActions(){
					{"取消", Dialog.Close},
					{"确定",  PayFor}
				});
			} 

			gameObject.GetComponent<DOPopup>().Close();
		});
	}

	private bool PayFor() {
		Commander.Shared.PayFor();
		return true;
	}
}
