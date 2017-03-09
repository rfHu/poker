using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Extensions;
using System;

[RequireComponent(typeof(DOPopup))]
public class UserDetail : MonoBehaviour {
	public RawImage Avatar;
	public Text Name;
	public Text Coins;
	public Text Hands;
	public Text ShowHand;
	public Text Join;
	public Text JoinWin; 
	public Text Aggressive;
	public Text PreRaise;
	public Text ThreeBet;
	public Text CBet;

	public void RequestById(string id) {
		var d = new Dictionary<string, object>(){
			{"uid", id}
		};

		Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", "gamerdetail"},
			{"args", d}
		}, (json) => {
			if (gameObject == null) {
				return ;
			}

			var data  = json.Dict("ret");
			var profile = data.Dict("profile");
			var achieve = data.Dict("achieve");
            
			Name.text = profile.String("name");
			Coins.text = achieve.String("coins");

			// 手数
			Hands.text = achieve.Int("total_hand_count").ToString();

			// 入池率
			Join.text = percent(achieve.Float("entry_hand_percent"));

			// 摊牌率
			ShowHand.text = percent(achieve.Float("showdown_hand_percent"));

			// 入池胜率
			JoinWin.text = percent(achieve.Float("entry_win_hand_percent"));

			// 激进度
			Aggressive.text = achieve.Float("aggressiveness").ToString();

			// 翻前加注
			PreRaise.text = percent(achieve.Float("pfr_hand_percent"));

			// 再次加注
			ThreeBet.text = percent(achieve.Float("t_bet_percent"));

			// 持续下注
			CBet.text = percent(achieve.Float("c_bet_round_percent"));

            _.DownloadImage(Avatar, profile.String("avatar"));
		});
	}

	private string percent(float number) {
		if (float.IsNaN(number) || float.IsInfinity(number)) { 
			number = 0;
		}

		return Math.Round(number * 100, 2).ToString() + "%"; 
	}
}
