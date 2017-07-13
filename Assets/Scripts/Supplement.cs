﻿using UnityEngine;
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

	private DialogAlert payDialog;

	// Use this for initialization
	void Awake() {
		RxSubjects.UnSeat.AsObservable().Where((e) => {
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

		slider.minValue = smin;
		slider.maxValue = smax;

		OnChange(smin);
	}

	public void OnChange(float value) {
		int step = GameData.Shared.BB * 100; 
		int newValue = value.StepValue(step);
		
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
				payDialog = PokerUI.Alert("金币不足，请购买", () => {
					Commander.Shared.PayFor();

					// 隐藏购买按钮
					payDialog.Hide();

					// 购买记分牌弹框
					RxSubjects.TakeCoin.OnNext(new RxData());
				}, null);
			} 

			GetComponent<DOPopup>().Close();
		});
	}
}
