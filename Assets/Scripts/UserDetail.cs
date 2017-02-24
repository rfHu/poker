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

	public void RequestById(string id) {
		var d = new Dictionary<string, object>(){
			{"uid", id}
		};

		Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", "gamerdetail"},
			{"args", d}
		}, (json) => {
			var data  = json.Dict("ret");
			var profile = data.Dict("profile");
			var achieve = data.Dict("achieve");
            
			Name.text = profile.String("name");
			Coins.text = achieve.String("coins");

			// 入池率
			var hands = achieve.Int("total_hand_count");
			var entry = achieve.Float("entry_hand_count");
			Hands.text = hands.ToString();
			Join.text = percent(entry / (float)hands);

			// 摊牌率
			float showHand = achieve.Float("showdown_hand_count") / achieve.Float("seecard_hand_count"); 
			ShowHand.text = percent(showHand);

			float win = achieve.Float("win_hand_count");
			JoinWin.text = percent(win / entry);

            StartCoroutine(downloadImage(Avatar, profile.String("avatar")));
		});
	}

	private string percent(float number) {
		if (float.IsNaN(number) || float.IsInfinity(number)) { 
			number = 0;
		}

		return Math.Round(number * 100, 2).ToString() + "%"; 
	}

	IEnumerator<WWW> downloadImage(RawImage img, string url) {
		WWW www = new WWW(url);
		yield return www;
		img.texture = _.Circular(www.texture);
	}	
}
