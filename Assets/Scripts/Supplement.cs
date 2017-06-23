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

	private bool ready = false;
	private DialogAlert payDialog;

	// Use this for initialization
	void Awake() {
		RxSubjects.UnSeat.AsObservable().Where((e) => {
			var uid = e.Data.String("uid");
			return GameData.Shared.Uid == uid;
		}).Subscribe((e) => {
			GetComponent<DOPopup>().Close();
		}).AddTo(this);
	}

	void OnSpawned() {
		Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", "gamerdetail"},
			{"args",  new Dictionary<string, object> {
				{"uid", GameData.Shared.Uid},
				{"bankroll_multiple", "1"}
			}}
		}, (ret) => {
			var coins = ret.Dict("achieve").Int("coins");

			GameData.Shared.Coins = coins; // 保存coins
			Coins.text = coins.ToString();

			var mul = ret.IL("bankroll_multiple"); 
			int score = GameData.Shared.BB * 100; 
			int min = mul[0] * score;
			int max = Math.Max(mul[0], mul[1]) * score;
			
			OnChange(min);

			slider.minValue = min;
			slider.maxValue = max;
			slider.onValueChanged.AddListener(OnChange);		

			ready = true;	
		});

		Coins.text = GameData.Shared.Coins.ToString();
		Blind.text = string.Format("{0}/{1}", GameData.Shared.BB / 2, GameData.Shared.BB);
	}

	public void OnChange(float value) {
		int step = GameData.Shared.BB * 100; 
		int newValue = value.StepValue(step);
		
		slider.value = newValue;

		Score.text = newValue.ToString();
		Pay.text = (newValue * GameData.Shared.Rake).ToString();
	}

	public void TakeCoin() {
		if (!ready) {
			return ;
		}

		float value = slider.value;	
		Connect.Shared.Emit(new Dictionary<string, object>(){
			{"f", "takecoin"},
			{"args", new Dictionary<string, object>{
				{"multiple", value / (100 * GameData.Shared.BB)}
			}}
		}, (json) => {
			var err = json.Int("err");
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
