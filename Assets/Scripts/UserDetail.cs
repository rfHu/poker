using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Extensions;

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
			var profile = data.Dict("profile").ToObject<ProfileModel>();
			var achieve = data.Dict("achieve").ToObject<AchieveModel>();
            
			Name.text = profile.name;
            _.DownloadImage(Avatar, profile.avatar);

			// 金币数
			Coins.text = achieve.coins;

			// 手数
			Hands.text = achieve.total_hand_count.ToString();

			// 入池率
			Join.text = _.PercentStr(achieve.entry_hand_percent);

			// 摊牌率
			ShowHand.text = _.PercentStr(achieve.showdown_hand_percent);

			// 入池胜率
			JoinWin.text = _.PercentStr(achieve.entry_win_hand_percent);

			// 激进度
			Aggressive.text = achieve.aggressiveness.ToString();

			// 翻前加注
			PreRaise.text = _.PercentStr(achieve.pfr_hand_percent);

			// 再次加注
			ThreeBet.text = _.PercentStr(achieve.t_bet_percent);

			// 持续下注
			CBet.text = _.PercentStr(achieve.c_bet_round_percent);
		});
	}
}
