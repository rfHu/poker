using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UniRx;
using System;
using MaterialUI;

[RequireComponent(typeof(DOPopup))]
public class Supplement : MonoBehaviour {

	public Text Blind;
	public Text Score;
	public Text Coins;
	public Text Pay;
	public Slider slider;

	void Awake() {
		RxSubjects.UnSeat.Where((e) => {
			var uid = e.Data.String("uid");
			return GameData.Shared.Uid == uid;
		}).Subscribe((e) => {
			GetComponent<DOPopup>().Close();
		}).AddTo(this);

		slider.onValueChanged.AddListener(OnChange);
	}

	void OnSpawned() {
		Coins.text = _.Num2CnDigit(GameData.Shared.Coins);
		Blind.text = string.Format("{0}/{1}", GameData.Shared.BB / 2, GameData.Shared.BB);

		var mul = GameData.Shared.BankrollMul;
		var bb100 = 100 * GameData.Shared.BB;
		var min = mul[0] * bb100;
		var max = mul[1] * bb100;
		var bankroll = GameData.Shared.Bankroll.Value;

		if (bankroll >= max) {
			return ;
		}

		int smin; 
		int smax;

		if (bankroll < min) {
			smin = Mathf.CeilToInt((min - bankroll) / (float)bb100) * bb100; 
		} else {
			smin = bb100;
		}

		smax = Mathf.CeilToInt((max - bankroll) / (float)bb100) * bb100;

		slider.maxValue = slider.minValue = slider.value  = 0;
		slider.maxValue = smax;
		slider.minValue = smin;

		OnChange(smin);
	}

	public void OnChange(float value) {
		int step = GameData.Shared.BB * 100; 
		int newValue = value.StepValue(step);

		// 解决赋值循环导致崩溃		
		if (newValue > slider.maxValue) {
			return ;
		}

		slider.value = newValue;
		Score.text = newValue.ToString();
		Pay.text = (newValue * GameData.Shared.Rake).ToString();
	}

	public void TakeCoin() {
		float value = slider.value;	
		Connect.Shared.Emit(new Dictionary<string, object>(){
			{"f", "takecoin"},
			{"args", new Dictionary<string, object>{
				{"multiple", value / (100 * GameData.Shared.BB)}
			}}
		}, (json, err) => {
			if (err == 1201) {
				_.PayFor(() => {
					RxSubjects.TakeCoin.OnNext(new RxData());
				});	
			} 

			GetComponent<DOPopup>().Close();
		});
	}
}
